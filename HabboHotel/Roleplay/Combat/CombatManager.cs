using Plus.Communication.Packets.Incoming;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay.Police;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Roleplay;
using System;
using System.Linq;

namespace Plus.HabboHotel.Roleplay.Combat
{
    public class CombatManager
    {
        private static readonly TimeSpan StunDuration = TimeSpan.FromSeconds(2);

        public bool AttackUser(Habbo attacker, Habbo target)
        {
            if (attacker == null || target == null)
                return false;

            // Rp Stats
            var attackerStats = attacker.GetRpStats();
            var targetStats = target.GetRpStats();
            if (attackerStats == null || targetStats == null)
                return false;

            // Room users
            RoomUser targetRoomUser = target.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(target.Id);
            RoomUser attackerRoomUser = attacker.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(attacker.Id);
            if (targetRoomUser == null || attackerRoomUser == null)
                return false;

            // No fighting while idle — an idle attacker can't swing, and an idle player can't be
            // jumped (stops people getting killed while AFK, and stops idlers from attacking).
            if (attackerRoomUser.IsAsleep) {
                attacker.GetClient()?.SendWhisper("You can't attack while you're idle.", 1);
                return false;
            }

            if (targetRoomUser.IsAsleep) {
                attacker.GetClient()?.SendWhisper(target.Username + " is idle — you can't attack them right now.", 1);
                return false;
            }

            if (attackerStats.IsDead || targetStats.IsDead) {
                attacker.GetClient().SendWhisper("User is already knocked out!", 1);
                return false;
            }

            UserWeapon activeWeapon = attackerStats.ActiveWeapon();
            var weapon = activeWeapon?.WeaponData;
            WeaponSkin skin = attacker.GetRpWeaponSkins()?.GetEquippedSkinForWeapon(activeWeapon?.WeaponId ?? 0)?.SkinData;

            // Tile-based reach: range 1 = same/adjacent tile, range 2 adds one tile of gap.
            // Fists (no weapon) are range 1 and may hit diagonally.
            int range = weapon?.Range ?? 1;
            bool allowDiagonal = weapon?.AllowDiagonal ?? true;

            if (!RpProximity.IsWithinWeaponRange(attackerRoomUser, targetRoomUser, range, allowDiagonal)) {
                attacker.CurrentRoom.SendPacket(new ShoutComposer(attackerRoomUser.VirtualId, "*swings at " + target.Username + ", but misses*", 0, 29, isRpAction: true));
                return false;
            }

            // Everything that could refuse the swing has passed, so it costs stamina from here on
            // — an out-of-reach swing shouldn't drain the attacker.
            attackerStats.InteractionEnergy();

            // The tazer is a pure-stun police tool: no damage, guaranteed 2s stun, limited charges,
            // and tazing is not treated as an assault crime.
            if (TazerService.IsTazer(weapon)) {
                HandleTazer(attacker, target, attackerStats, attackerRoomUser, targetRoomUser);
                return true;
            }

            // Damage user
            DamageResult result = CalculateDamage(weapon?.MinimumDamage ?? 0, weapon?.MaximumDamage ?? 0, weapon?.CriticalChance ?? 0, weapon?.CriticalMultiplier ?? 1, attackerStats.Strength);

            int totalDamage = result.TotalDamage;

            // A standard vest flat-reduces non-fist hits (and takes durability doing so).
            bool isFistAttack = (weapon?.Id ?? 0) == 0;
            totalDamage = target.GetRpItems()?.AbsorbWithVest(target, totalDamage, isFistAttack) ?? totalDamage;

            int shieldBeforeHit = targetStats.Shield;
            int healthBeforeHit = targetStats.Health;
            int absorbedByShield = Math.Min(shieldBeforeHit, totalDamage);
            int damageToHealth = Math.Min(healthBeforeHit, Math.Max(0, totalDamage - shieldBeforeHit));
            int actualDamageDealt = absorbedByShield + damageToHealth;
            bool isFinalBlow = damageToHealth >= healthBeforeHit && healthBeforeHit > 0;
            int experience = 0;

            UserWeapon equippedWeapon = attacker.GetRpWeapons()?.GetActiveWeapon();
            if (equippedWeapon != null && !equippedWeapon.IsDefault && equippedWeapon.DurabilityLeft > 0) {
                equippedWeapon.DurabilityLeft--;

                if (equippedWeapon.DurabilityLeft <= 0) {
                    attacker.GetRpWeapons()?.RemoveWeapon(equippedWeapon.Id);
                    attacker.SaveRpWeapons();
                    attacker.GetClient()?.SendWhisper("Your " + (equippedWeapon.WeaponData?.Name ?? "weapon") + " broke.", 1);
                    attackerRoomUser.ApplyEffect(0);
                }

                WebOverlayCallbackEvent.RefreshInventory(attacker.GetClient());
            }

            // stats and damage application
            targetStats.Shield = Math.Max(0, targetStats.Shield - totalDamage);

            if (damageToHealth > 0) {
                targetStats.Health -= damageToHealth;
            }

            if (isFinalBlow) {
                experience = 4;
                attacker.ResetAggressionTimer();
                targetStats.Health = 0;
                LiveFeedService.LiveFeed(LiveFeedService.Name(attacker.Username, "green") + " knocked out <b>" + LiveFeedService.Name(target.Username, "red") + "</b>");
                UserRpStats.Kill(target, targetRoomUser, attackerStats);

                // Resolve which murder-family crime applies (the livefeed for auto crimes is
                // emitted from RpCrimeManager.Commit).
                CommitKillCrimes(attacker, target);

                // Achievements (batched: a fight can produce bursts of these).
                PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(attacker.GetClient(), "ACH_Kill", 1);
                PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(target.GetClient(), "ACH_Death", 1);
            } else {
                experience = 2;
                attacker.ResetAggressionTimer();

                // A non-lethal hit is an assault (or a more specific variant).
                CommitAssaultCrime(attacker, target, attacker.CurrentRoom);
            }

            // Send shout about the attack after damage is resolved so it reflects the final result.
            string shoutResult = isFinalBlow
                ? "*deals the final blow to " + target.Username + " with " + actualDamageDealt + " damage*" : FormatAttackMessage(attacker.Username, skin, weapon, target.Username, actualDamageDealt, result.IsCritical);
            attacker.CurrentRoom.SendPacket(new ShoutComposer(attackerRoomUser.VirtualId, shoutResult, 0, 29, isRpAction: true));

            // stun
            bool stunTarget = Random.Shared.NextDouble() < ((weapon?.StunChance ?? 0) / 100.0);
            if (stunTarget && !targetStats.IsDead) {
                target.CurrentRoom.SendPacket(new ShoutComposer(attacker.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(target.Id).VirtualId, "*gets stunned by the hit*", 0, 29, isRpAction: true));
                StunUser(targetRoomUser);
            }

            attackerStats.Attacks++;
            attackerStats.DamageDealt += actualDamageDealt;

            attackerStats.Experience += experience;

            targetStats.Attacked++;
            targetStats.DamageTaken += actualDamageDealt;

            return true;
        }

        private static readonly string[] MurderTags = { "murder", "cop_murder", "gang_homicide" };

        private static void CommitKillCrimes(Habbo attacker, Habbo target)
        {
            var crimes = PlusEnvironment.GetRpCrimeManager();

            // Checked before committing this kill, so it reflects the attacker's *prior* murders.
            bool alreadyMurderer = MurderTags.Any(tag => crimes.HasActiveCrime(attacker.Id, tag));

            string tag;
            if (PoliceManager.IsOnDutyPolice(target))
                tag = "cop_murder";
            else if (IsInGang(attacker) && IsInGang(target))
                tag = "gang_homicide";
            else
                tag = "murder";

            crimes.Commit(attacker, tag, attacker.CurrentRoom);

            // A second murder while a murder charge is still active makes it a mass murder.
            if (alreadyMurderer)
                crimes.Commit(attacker, "mass_murder", attacker.CurrentRoom);
        }

        private static void CommitAssaultCrime(Habbo attacker, Habbo target, Room room)
        {
            string tag;
            if (PoliceManager.IsOnDutyPolice(target))
                tag = "cop_assault";
            else if (PlusEnvironment.GetPoliceManager().IsEscorted(target.Id))
                tag = "obstruction";
            else
                tag = "assault";

            PlusEnvironment.GetRpCrimeManager().Commit(attacker, tag, room);
        }

        private static bool IsInGang(Habbo habbo)
        {
            if (habbo == null)
                return false;

            foreach (Group group in PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(habbo.Id)) {
                if (group != null && (group.Kind == GroupKind.Gang || group.Kind == GroupKind.Cartel || group.Kind == GroupKind.Mafia))
                    return true;
            }
            return false;
        }

        public void ApplyExplosion(Habbo source, int minDamage, int maxDamage)
        {
            if (source == null)
                return;

            Room room = source.CurrentRoom;
            RoomUser sourceUser = room?.GetRoomUserManager().GetRoomUserByHabbo(source.Id);
            if (room == null || sourceUser == null)
                return;

            minDamage = Math.Max(0, minDamage);
            maxDamage = Math.Max(minDamage, maxDamage);

            room.SendPacket(new ShoutComposer(sourceUser.VirtualId, "*detonates a suicide vest!*", 0, 29, isRpAction: true));
            LiveFeedService.LiveFeed("💥 " + LiveFeedService.Name(source.Username, "red") + " set off a suicide vest!", "red");

            source.GetRpStats().Experience += 10;

            foreach (RoomUser ru in room.GetRoomUserManager().GetRoomUsers()) {
                if (ru == null || ru.IsBot || ru.HabboId == source.Id)
                    continue;

                // the blast reaches out to a 2-tile radius (5x5).
                int distance = Math.Max(Math.Abs(ru.X - sourceUser.X), Math.Abs(ru.Y - sourceUser.Y));
                if (distance > 2)
                    continue;

                Habbo victim = ru.GetClient()?.GetHabbo();
                UserRpStats victimStats = victim?.GetRpStats();
                if (victim == null || victimStats == null || victimStats.IsDead)
                    continue;

                // Passive players are immune to the blast.
                if (victimStats.PassiveMode || victimStats.IsGodMode)
                    continue;

                if (distance <= 1)
                    ApplyLethalBlast(victim, ru, victimStats, source);
                else
                    ApplyBlastDamage(victim, ru, victimStats, Random.Shared.Next(minDamage, maxDamage + 1), source);
            }

            // The bomber always dies.
            UserRpStats sourceStats = source.GetRpStats();
            if (sourceStats != null && !sourceStats.IsDead) {
                UserRpStats.Kill(source, sourceUser);
                PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(source.GetClient(), "ACH_Death", 1);
            }
        }

        private static void ApplyLethalBlast(Habbo victim, RoomUser victimUser, UserRpStats victimStats, Habbo triggeredBy)
        {
            int absorbed = victimStats.Shield + victimStats.Health;
            victimStats.Shield = 0;
            victimStats.Health = 0;
            victimStats.DamageTaken += absorbed;
            triggeredBy.GetRpStats().DamageDealt += absorbed;
            triggeredBy.GetRpStats().Knockouts += 1;

            victim.CurrentRoom?.SendPacket(new ShoutComposer(victimUser.VirtualId, "*was caught in the explosion with no mercy*", 0, 29, isRpAction: true));
            LiveFeedService.LiveFeed("💥 " + LiveFeedService.Name(victim.Username, "red") + " was caught in a blast by " + LiveFeedService.Name(triggeredBy.Username, "green"), "red");
            UserRpStats.Kill(victim, victimUser);

            PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(triggeredBy.GetClient(), "ACH_Kill", 1);
            PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(victim.GetClient(), "ACH_Death", 1);

            victim.TrySendUserStatsUpdate(true);
        }

        private static void ApplyBlastDamage(Habbo victim, RoomUser victimUser, UserRpStats victimStats, int totalDamage, Habbo triggeredBy)
        {
            int shieldBefore = victimStats.Shield;
            int healthBefore = victimStats.Health;
            int damageToHealth = Math.Min(healthBefore, Math.Max(0, totalDamage - shieldBefore));
            bool lethal = damageToHealth >= healthBefore && healthBefore > 0;

            victimStats.Shield = Math.Max(0, victimStats.Shield - totalDamage);
            victimStats.DamageTaken += totalDamage;
            triggeredBy.GetRpStats().DamageDealt += totalDamage;

            if (damageToHealth > 0)
                victimStats.Health -= damageToHealth;

            // Everyone caught in the blast announces the hit.
            victim.CurrentRoom?.SendPacket(new ShoutComposer(victimUser.VirtualId, "*took " + totalDamage + " explosive damage*", 0, 29, isRpAction: true));

            if (lethal) {
                victimStats.Health = 0;
                triggeredBy.GetRpStats().Knockouts += 1;
                LiveFeedService.LiveFeed("💥 " + LiveFeedService.Name(victim.Username, "red") + " was caught in a blast by " + LiveFeedService.Name(triggeredBy.Username, "green"), "red");
                UserRpStats.Kill(victim, victimUser);

                // achievements n such
                PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(triggeredBy.GetClient(), "ACH_Kill", 1);
                PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(victim.GetClient(), "ACH_Death", 1);
            }

            victim.TrySendUserStatsUpdate(true);
        }

        private DamageResult CalculateDamage(int minDamage, int maxDamage, double critChancePercent, double critMultiplier, int strength)
        {
            // this is a bit of a cleanup, but this is to calculate all the variables and return a clean object
            minDamage = Math.Max(0, minDamage);
            maxDamage = Math.Max(minDamage, maxDamage);

            int baseDamage = Random.Shared.Next(minDamage, maxDamage + 1);

            bool isCrit = Random.Shared.NextDouble() < (critChancePercent / 100.0);

            double damageAfterCrit = baseDamage * (isCrit ? critMultiplier : 1);

            int totalDamage = (int)Math.Ceiling(damageAfterCrit) + UserRpStats.GetAttributeBonus(strength);

            return new DamageResult
            {
                TotalDamage = totalDamage,
                IsCritical = isCrit
            };
        }

        private void HandleTazer(Habbo attacker, Habbo target, UserRpStats attackerStats, RoomUser attackerRoomUser, RoomUser targetRoomUser)
        {
            UserWeapon tazer = attacker.GetRpWeapons()?.GetActiveWeapon();
            if (tazer == null || tazer.DurabilityLeft <= 0) {
                attacker.GetClient()?.SendWhisper("Your tazer is out of charge — recharge it at the station.", 1);
                return;
            }

            tazer.DurabilityLeft--;
            attacker.GetRpWeapons()?.MarkDirty();

            attacker.ResetAggressionTimer();
            attacker.CurrentRoom.SendPacket(new ShoutComposer(attackerRoomUser.VirtualId, "*tazes " + target.Username + ", stunning them*", 0, 29, isRpAction: true));

            if (!target.GetRpStats().IsDead)
                StunUser(targetRoomUser);

            attackerStats.Attacks++;
        }

        private static void StunUser(RoomUser targetRoomUser)
        {
            if (targetRoomUser == null)
                return;

            targetRoomUser.ApplyTimedFreeze(StunDuration, 53);
        }

        private static string FormatAttackMessage(string attackerUsername, WeaponSkin skin, Weapon weapon, string targetUsername, int damage, bool isCritical)
        {
            string message = isCritical
                ? skin?.CriticalHitMessage ?? weapon?.CriticalHitMessage ?? "*lands a critical hit on %username%, dealing %damage% damage*"
                : skin?.HitMessage ?? weapon?.HitMessage ?? "*swings at %username%, dealing %damage% damage*";

            message = message.Replace("%attacker%", attackerUsername).Replace("%username%", targetUsername).Replace("%damage%", damage.ToString());

            return message;
        }

        public void RobUser(Habbo attacker, Habbo target, Room room)
        {
            if (attacker == null || target == null || room == null)
                return;

            // stats
            var attackerStats = attacker.GetRpStats();
            var targetStats = target.GetRpStats();
            if (attackerStats == null || targetStats == null)
                return;

            // take away energy
            attackerStats.InteractionEnergy();

            bool alwaysSucceeds = targetStats.IsDead;

            if (target.Credits <= 0 || (!alwaysSucceeds && UserRpStats.GetRobberyChance(attacker) < Random.Shared.Next(0, 100))) {
                attacker.CurrentRoom.SendPacket(new ShoutComposer(attacker.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(attacker.Id).VirtualId, "*attempts to rob " + target.Username + ", but fails*", 0, 6, isRpAction: true));
                return;
            }

            int amountToSteal = Random.Shared.Next(1, target.Credits + 1);

            attacker.Credits += amountToSteal;
            target.Credits -= amountToSteal;

            // packets
            target.GetClient().SendPacket(new CreditBalanceComposer(target.Credits));
            attacker.GetClient().SendPacket(new CreditBalanceComposer(attacker.Credits));

            // stats
            targetStats.Robbed++;
            attackerStats.Robberies++;
            attacker.GetRpSkills().ProgressSkill(3, 1);
            attackerStats.Experience += 3;

            LiveFeedService.LiveFeed(LiveFeedService.Name(attacker.Username, "green") + " robbed <b>" + LiveFeedService.Name(target.Username, "red") + "</b>");

            // a successful robbery is theft
            PlusEnvironment.GetRpCrimeManager().Commit(attacker, "rob", room);

            // Achievement: successful robbery of another player.
            PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(attacker.GetClient(), "ACH_Robber", 1);

            // notification
            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(attacker.Id).VirtualId, "*robs " + target.Username + " of " + amountToSteal + " credits*", 0, 6, isRpAction: true));
            return;
        }
    }
}