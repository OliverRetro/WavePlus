using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using System;

namespace Plus.HabboHotel.Roleplay.Combat
{
    public class CombatManager
    {
        private static readonly TimeSpan StunDuration = TimeSpan.FromSeconds(1.5);

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
                attacker.CurrentRoom.SendPacket(new ShoutComposer(attacker.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(attacker.Id).VirtualId, "*Misses " + target.Username + "*", 0, 23));
                return;
            }

            // stun
            bool stunTarget = Random.Shared.NextDouble() < ((weapon?.StunChance ?? 0) / 100.0);
            if (stunTarget)
            {
                target.CurrentRoom.SendPacket(new ShoutComposer(attacker.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(target.Id).VirtualId, "*Got stunned*", 0, 23));
                StunUser(targetRoomUser);
            }

            // Damage user
            DamageResult result = CalculateDamage(weapon?.MinimumDamage ?? 0, weapon?.MaximumDamage ?? 0, weapon?.CriticalChance ?? 0, weapon?.CriticalMultiplier ?? 1, attackerStats.Strength);

            int totalDamage = result.TotalDamage;

            // Send shout about the attack
            string shoutResult = result.IsCritical ? "*CRITICAL HIT! " + target.Username + " takes " + totalDamage + " damage!*" : "*hits " + target.Username + " for " + totalDamage + " damage*";
            attacker.CurrentRoom.SendPacket(new ShoutComposer(attacker.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(attacker.Id).VirtualId, shoutResult, 0, 23));

            attacker.GetRpWeapons().GetActiveWeapon().DurabilityLeft--;

            // stats and damage application
            int remainingDamage = totalDamage - targetStats.Shield;
            targetStats.Shield = Math.Max(0, targetStats.Shield - totalDamage);

            if (remainingDamage > 0)
            {
                targetStats.Health = Math.Max(0, targetStats.Health - remainingDamage);
            }

            if (targetStats.Health <= 0)
            {
                targetStats.IsDead = true;
            }

            attackerStats.Attacks++;
            attackerStats.DamageDealt += totalDamage;

            targetStats.Attacked++;
            targetStats.DamageTaken += totalDamage;
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

            targetRoomUser.ApplyTimedFreeze(StunDuration);
        }
    }
}
