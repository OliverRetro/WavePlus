using Plus.Communication.Packets.Outgoing.Rooms.Chat;
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

            // Room users
            RoomUser targetRoomUser = target.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(target.Id);
            RoomUser attackerRoomUser = attacker.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(attacker.Id);

            if (attackerStats.IsDead || targetStats.IsDead)
                return;

            var weapon = attackerStats.ActiveWeapon()?.WeaponData;

            int range = weapon?.Range ?? 1;

            int dx = Math.Abs(attackerRoomUser.X - targetRoomUser.X);
            int dy = Math.Abs(attackerRoomUser.Y - targetRoomUser.Y);

            double distance = Math.Sqrt(dx * dx + dy * dy);
            bool inRange = (dx + dy) <= range;

            if (!inRange)
            {
                attacker.CurrentRoom.SendPacket(new ShoutComposer(attacker.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(attacker.Id).VirtualId, "*swings at " + target.Username + ", but misses*", 0, 29));
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

            attacker.GetRpWeapons().GetActiveWeapon().DurabilityLeft--;

            // stats and damage application
            targetStats.Shield = Math.Max(0, targetStats.Shield - totalDamage);

            if (damageToHealth > 0)
            {
                targetStats.Health -= damageToHealth;
            }

            if (isFinalBlow)
            {
                targetStats.Health = 0;
                attackerStats.Kill(target, targetRoomUser);
            }

            // Send shout about the attack after damage is resolved so it reflects the final result.
            string shoutResult = isFinalBlow
                ? "*deals the final blow to " + target.Username + "*"
                : result.IsCritical
                    ? "*lands a critical hit on " + target.Username + ", dealing " + actualDamageDealt + " damage*"
                    : "*swings at " + target.Username + ", dealing " + actualDamageDealt + " damage*";
            attacker.CurrentRoom.SendPacket(new ShoutComposer(attackerRoomUser.VirtualId, shoutResult, 0, 29));

            // stun
            bool stunTarget = Random.Shared.NextDouble() < ((weapon?.StunChance ?? 0) / 100.0);
            if (stunTarget && !targetStats.IsDead)
            {
                target.CurrentRoom.SendPacket(new ShoutComposer(attacker.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(target.Id).VirtualId, "*Gets stunned by the hit*", 0, 29));
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
    }
}
