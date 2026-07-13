using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using System;

namespace Plus.HabboHotel.Roleplay.Combat
{
    public class CombatManager
    {
        private static readonly TimeSpan StunDuration = TimeSpan.FromSeconds(2);

        public void AttackUser(Habbo attacker, Habbo target)
        {
            if (attacker == null || target == null)
                return;

            // Rp Stats
            var attackerStats = attacker.GetRpStats();
            var targetStats = target.GetRpStats();
            if (attackerStats == null || targetStats == null)
                return;

            // take away energy
            attackerStats.InteractionEnergy();

            // Room users
            RoomUser targetRoomUser = target.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(target.Id);
            RoomUser attackerRoomUser = attacker.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(attacker.Id);
            if (targetRoomUser == null || attackerRoomUser == null)
                return;

            if (attackerStats.IsDead || targetStats.IsDead)
            {
                attacker.GetClient().SendWhisper("User is already knocked out!", 1);
                return;
            }

            UserWeapon activeWeapon = attackerStats.ActiveWeapon();
            var weapon = activeWeapon?.WeaponData;
            WeaponSkin skin = attacker.GetRpWeaponSkins()?.GetEquippedSkinForWeapon(activeWeapon?.WeaponId ?? 0)?.SkinData;

            int range = weapon?.Range ?? 1;

            int dx = Math.Abs(attackerRoomUser.X - targetRoomUser.X);
            int dy = Math.Abs(attackerRoomUser.Y - targetRoomUser.Y);

            double distance = Math.Sqrt(dx * dx + dy * dy);
            bool inRange = (dx + dy) <= range;

            if (!inRange)
            {
                attacker.CurrentRoom.SendPacket(new ShoutComposer(attacker.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(attacker.Id).VirtualId, "*" + attacker.Username + " swings at " + target.Username + ", but misses*", 0, 29));
                return;
            }

            // Damage user
            DamageResult result = CalculateDamage(weapon?.MinimumDamage ?? 0, weapon?.MaximumDamage ?? 0, weapon?.CriticalChance ?? 0, weapon?.CriticalMultiplier ?? 1, attackerStats.Strength);

            int totalDamage = result.TotalDamage;

            int shieldBeforeHit = targetStats.Shield;
            int healthBeforeHit = targetStats.Health;
            int absorbedByShield = Math.Min(shieldBeforeHit, totalDamage);
            int damageToHealth = Math.Min(healthBeforeHit, Math.Max(0, totalDamage - shieldBeforeHit));
            int actualDamageDealt = absorbedByShield + damageToHealth;
            bool isFinalBlow = damageToHealth >= healthBeforeHit && healthBeforeHit > 0;

            UserWeapon equippedWeapon = attacker.GetRpWeapons()?.GetActiveWeapon();
            if (equippedWeapon != null && equippedWeapon.DurabilityLeft > 0)
                equippedWeapon.DurabilityLeft--;

            // stats and damage application
            targetStats.Shield = Math.Max(0, targetStats.Shield - totalDamage);

            if (damageToHealth > 0)
            {
                targetStats.Health -= damageToHealth;
            }

            if (isFinalBlow)
            {
                attacker.ResetAggressionTimer();
                targetStats.Health = 0;
                LiveFeedService.LiveFeed(LiveFeedService.Name(attacker.Username, "green") + " knocked out <b>" + LiveFeedService.Name(target.Username, "red") + "</b>");
                LiveFeedService.LiveFeed("[AUTO CHARGE] 🚨 " + LiveFeedService.Name(attacker.Username, "green") + " committed murder ⭐⭐");
                UserRpStats.Kill(target, targetRoomUser, attackerStats);

                // Killing a player is murder.
                PlusEnvironment.GetRpCrimeManager().Commit(attacker, "murder", attacker.CurrentRoom);

                // Achievements (batched: a fight can produce bursts of these).
                PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(attacker.GetClient(), "ACH_Kill", 1);
                PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(target.GetClient(), "ACH_Death", 1);
            }
            else
            {
                attacker.ResetAggressionTimer();
            }

            // Send shout about the attack after damage is resolved so it reflects the final result.
            string shoutResult = isFinalBlow
                ? "*deals the final blow to " + target.Username + " with " + actualDamageDealt + " damage*" : FormatAttackMessage(attacker.Username, skin, weapon, target.Username, actualDamageDealt, result.IsCritical);
            attacker.CurrentRoom.SendPacket(new ShoutComposer(attackerRoomUser.VirtualId, shoutResult, 0, 29));

            // stun
            bool stunTarget = Random.Shared.NextDouble() < ((weapon?.StunChance ?? 0) / 100.0);
            if (stunTarget && !targetStats.IsDead)
            {
                target.CurrentRoom.SendPacket(new ShoutComposer(attacker.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(target.Id).VirtualId, "*" + target.Username + " gets stunned by the hit*", 0, 29));
                StunUser(targetRoomUser);
            }

            attackerStats.Attacks++;
            attackerStats.DamageDealt += actualDamageDealt;

            targetStats.Attacked++;
            targetStats.DamageTaken += actualDamageDealt;
        }

        private DamageResult CalculateDamage(int minDamage, int maxDamage, double critChancePercent, double critMultiplier, int strength)
        {
            // this is a bit of a cleanup, but this is to calculate all the variables and return a clean object
            minDamage = Math.Max(0, minDamage);
            maxDamage = Math.Max(minDamage, maxDamage);

            int baseDamage = Random.Shared.Next(minDamage, maxDamage + 1);

            bool isCrit = Random.Shared.NextDouble() < (critChancePercent / 100.0);

            double damageAfterCrit = baseDamage * (isCrit ? critMultiplier : 1);

            int totalDamage = (int)Math.Ceiling(damageAfterCrit) + strength * 2;

            return new DamageResult
            {
                TotalDamage = totalDamage,
                IsCritical = isCrit
            };
        }

        private void StunUser(RoomUser targetRoomUser)
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

            if (message.StartsWith("*") && !message.StartsWith("*" + attackerUsername + " "))
                message = "*" + attackerUsername + " " + message.Substring(1);

            return message;
        }

        public void RobUser(Habbo attacker, Habbo target, Room room, int percentage)
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

            // the chance of success
            if (UserRpStats.GetRobberyChance(attacker) < Random.Shared.Next(0, 100) || target.Credits <= 0)
            {
                attacker.CurrentRoom.SendPacket(new ShoutComposer(attacker.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(attacker.Id).VirtualId, "*" + attacker.Username + " attempts to rob " + target.Username + ", but fails*", 0, 6));
                return;
            }

            int amountToSteal = (int)Math.Round(target.Credits * (percentage / 100.0));

            // stolen credits are added to the attacker and removed from the target
            attacker.Credits = attacker.Credits + amountToSteal;
            target.Credits = target.Credits - amountToSteal;

            // packets
            target.GetClient().SendPacket(new CreditBalanceComposer(target.Credits));
            attacker.GetClient().SendPacket(new CreditBalanceComposer(attacker.Credits));

            // stats
            targetStats.Robbed++;
            attackerStats.Robberies++;
            attacker.GetRpSkills().ProgressSkill(3, 1);

            LiveFeedService.LiveFeed(LiveFeedService.Name(attacker.Username, "green") + " robbed <b>" + LiveFeedService.Name(target.Username, "red") + "</b>");

            // a successful robbery is theft
            PlusEnvironment.GetRpCrimeManager().Commit(attacker, "rob", room);

            // Achievement: successful robbery of another player.
            PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(attacker.GetClient(), "ACH_Robber", 1);

            // notification
            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(attacker.Id).VirtualId, "*robs " + target.Username + " of " + amountToSteal + " credits*", 0, 6));
            return;
        }
    }
}
