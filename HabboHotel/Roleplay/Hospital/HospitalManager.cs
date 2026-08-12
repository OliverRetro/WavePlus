using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Roleplay;

namespace Plus.HabboHotel.Roleplay.Hospital
{
    public class HospitalManager
    {
        private readonly ConcurrentDictionary<int, byte> _deadUsers = new();
        private readonly ConcurrentDictionary<int, double> _bleedoutUsers = new();

        public int GetHospitalRoomId()
        {
            return int.TryParse(PlusEnvironment.GetSettingsManager().TryGetValue("rp.hospital.room.id"), out int roomId)
                ? roomId
                : 0;
        }

        public int GetDeathDurationSeconds()
        {
            if (int.TryParse(PlusEnvironment.GetSettingsManager().TryGetValue("rp.hospital.death.seconds"), out int seconds) && seconds > 0)
                return seconds;

            return 120;
        }

        public bool IsTrackedDead(int userId)
        {
            return _deadUsers.ContainsKey(userId);
        }

        public void HandleDeath(Habbo target, RoomUser targetUser, UserRpStats attackerStats = null)
        {
            if (target == null || targetUser == null)
                return;

            UserRpStats targetStats = target.GetRpStats();
            if (targetStats == null)
                return;

            double now = PlusEnvironment.GetUnixTimestamp();

            targetStats.IsDead = true;
            targetStats.Deaths++;
            targetStats.HospitalReleaseTime = 0;
            targetStats.HospitalHealStartTime = 0;
            targetStats.HospitalHealthStart = 0;
            targetStats.NextPassiveHospitalHealTick = 0;

            if (attackerStats != null)
                attackerStats.Knockouts++;

            _deadUsers[target.Id] = 0;
            _bleedoutUsers[target.Id] = now + GetDeathDurationSeconds();

            // Death ends any active work shift.
            PlusEnvironment.GetGame().GetShiftManager().InterruptShift(target, target.CurrentRoom);

            // Death frees any suspects this user was escorting and knocks off their own cuffs.
            PlusEnvironment.GetPoliceManager()?.OnDeath(target);

            RpWeaponService.Disarm(target);

            // A corpse can't keep healing — drop any in-progress slow heal.
            PlusEnvironment.GetSlowHealManager()?.Cancel(target.Id);

            targetUser.ClearMovement(true);
            if (!targetUser.IsLying)
                targetUser.Lay();
            target.CurrentRoom?.GetRoomUserManager().UpdateUserStatus(targetUser, false);
            target.SaveRpStats();

            // Death changes which items are usable, so the open inventory has to re-grey.
            if (target.GetClient() != null)
                Plus.Communication.Packets.Incoming.WebOverlayCallbackEvent.RefreshInventory(target.GetClient());
        }

        public void SkipBleedout(Habbo habbo)
        {
            if (habbo?.GetRpStats()?.IsDead != true)
                return;

            _bleedoutUsers.TryRemove(habbo.Id, out _);
            BeginHospitalStay(habbo, true);
        }

        public void CancelDeath(Habbo habbo)
        {
            if (habbo?.GetRpStats() == null)
                return;

            UserRpStats stats = habbo.GetRpStats();
            stats.IsDead = false;
            stats.HospitalReleaseTime = 0;
            stats.HospitalHealStartTime = 0;
            stats.HospitalHealthStart = 0;
            stats.NextPassiveHospitalHealTick = 0;

            // Back among the living — un-grey the inventory.
            if (habbo.GetClient() != null)
                Plus.Communication.Packets.Incoming.WebOverlayCallbackEvent.RefreshInventory(habbo.GetClient());

            _deadUsers.TryRemove(habbo.Id, out _);
            _bleedoutUsers.TryRemove(habbo.Id, out _);

            if (habbo.CurrentRoom == null) {
                habbo.SaveRpStats();
                return;
            }

            RoomUser roomUser = habbo.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (roomUser != null) {
                roomUser.ClearMovement(true);
                roomUser.RemoveStatus("lay");
                roomUser.RemoveStatus("sit");
                roomUser.IsLying = false;
                roomUser.IsSitting = false;
                roomUser.UpdateNeeded = true;
                habbo.CurrentRoom.GetRoomUserManager().UpdateUserStatus(roomUser, false);
            }

            habbo.SaveRpStats();
        }

        public void HandleUserCycle(Habbo habbo)
        {
            if (habbo == null)
                return;

            UserRpStats stats = habbo.GetRpStats();
            if (stats == null)
                return;

            if (!stats.IsDead && stats.Health <= 0) {
                RoomUser downed = habbo.CurrentRoom?.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
                if (downed != null)
                    UserRpStats.Kill(habbo, downed);
            }

            if (stats.IsDead) {
                _deadUsers[habbo.Id] = 0;
                if (stats.HospitalReleaseTime > 0)
                    ProgressDeadUserRecovery(habbo);
                else
                    ProgressBleedout(habbo);
                return;
            }

            _deadUsers.TryRemove(habbo.Id, out _);
            _bleedoutUsers.TryRemove(habbo.Id, out _);
            HandlePassiveHospitalHeal(habbo);
        }

        public void OnCycle()
        {
            foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients) {
                if (client?.GetHabbo() == null)
                    continue;

                client.GetHabbo().TrySendUserStatsUpdate();
                HandleUserCycle(client.GetHabbo());
            }
        }

        public void HandleRoomEntry(Habbo habbo)
        {
            if (habbo == null || habbo.CurrentRoom == null)
                return;

            if (habbo.GetRpStats()?.IsDead != true)
                return;

            if (habbo.CurrentRoom.RoomId != GetHospitalRoomId())
                return;

            habbo.PendingReconnectPlacement = false;
            PlaceOnHospitalBed(habbo);
        }

        private void ProgressDeadUserRecovery(Habbo habbo)
        {
            UserRpStats stats = habbo.GetRpStats();
            if (stats == null)
                return;

            double releaseTime = stats.HospitalReleaseTime;
            if (releaseTime <= 0)
                return;

            if (habbo.CurrentRoom == null || habbo.CurrentRoom.RoomId != GetHospitalRoomId())
                return;

            int maxHealth = UserRpStats.GetMaxHealth(habbo);
            double now = PlusEnvironment.GetUnixTimestamp();
            double totalDuration = Math.Max(1, releaseTime - stats.HospitalHealStartTime);
            double elapsed = Math.Clamp(now - stats.HospitalHealStartTime, 0, totalDuration);
            int healedHealth = stats.HospitalHealthStart + (int)Math.Floor((maxHealth - stats.HospitalHealthStart) * (elapsed / totalDuration));
            stats.Health = Math.Clamp(healedHealth, 0, maxHealth);

            if (now < releaseTime) {
                return;
            }

            stats.Health = maxHealth;
            stats.IsDead = false;
            stats.HospitalReleaseTime = 0;
            stats.HospitalHealStartTime = 0;
            stats.HospitalHealthStart = 0;
            stats.NextPassiveHospitalHealTick = PlusEnvironment.GetUnixTimestamp() + 3;
            _deadUsers.TryRemove(habbo.Id, out _);

            Discharge(habbo);
            habbo.SaveRpStats();

            // Discharged — un-grey the inventory.
            if (habbo.GetClient() != null)
                Plus.Communication.Packets.Incoming.WebOverlayCallbackEvent.RefreshInventory(habbo.GetClient());
        }

        private void HandlePassiveHospitalHeal(Habbo habbo)
        {
            if (habbo.CurrentRoom == null || habbo.CurrentRoom.RoomId != GetHospitalRoomId() || habbo.GetRpStats().IsDead)
                return;

            UserRpStats stats = habbo.GetRpStats();
            int maxHealth = UserRpStats.GetMaxHealth(habbo);
            if (stats.Health >= maxHealth)
                return;

            double now = PlusEnvironment.GetUnixTimestamp();
            if (stats.NextPassiveHospitalHealTick <= 0)
                stats.NextPassiveHospitalHealTick = now + 3;

            if (now < stats.NextPassiveHospitalHealTick)
                return;

            stats.Health = Math.Min(maxHealth, stats.Health + 1);
            stats.NextPassiveHospitalHealTick = now + 3;
            habbo.SaveRpStats();
        }

        public int GetLoginRoomId(Habbo habbo)
        {
            if (habbo == null)
                return 0;

            int hospitalRoomId = GetHospitalRoomId();
            if (hospitalRoomId <= 0 || habbo.GetRpStats()?.IsDead != true)
                return 0;

            if (habbo.GetRpStats().HospitalReleaseTime <= 0) {
                _bleedoutUsers.TryRemove(habbo.Id, out _);
                BeginHospitalStay(habbo, false);
            }

            return hospitalRoomId;
        }

        public bool IsRestrictedHospitalBedTile(Room room, Habbo habbo, int x, int y)
        {
            if (room == null || habbo == null)
                return false;

            if (room.RoomId != GetHospitalRoomId() || habbo.GetRpStats()?.IsDead == true)
                return false;

            List<Item> items = room.GetGameMap().GetAllRoomItemForSquare(x, y);
            return items.Any(item => item?.GetBaseItem()?.InteractionType == InteractionType.Bed);
        }

        private void ProgressBleedout(Habbo habbo)
        {
            UserRpStats stats = habbo.GetRpStats();
            if (stats == null)
                return;

            if (!_bleedoutUsers.TryGetValue(habbo.Id, out double bleedoutTime))
                return;

            if (PlusEnvironment.GetUnixTimestamp() < bleedoutTime)
                return;

            _bleedoutUsers.TryRemove(habbo.Id, out _);
            BeginHospitalStay(habbo, true);
        }

        private void BeginHospitalStay(Habbo habbo, bool sendImmediately)
        {
            UserRpStats stats = habbo?.GetRpStats();
            if (stats == null)
                return;

            double now = PlusEnvironment.GetUnixTimestamp();
            stats.Health = 0;
            stats.HospitalReleaseTime = now + GetDeathDurationSeconds();
            stats.HospitalHealStartTime = now;
            stats.HospitalHealthStart = 0;
            stats.NextPassiveHospitalHealTick = 0;
            habbo.SaveRpStats();

            if (sendImmediately)
                TrySendToHospital(habbo);
        }

        private void TrySendToHospital(Habbo habbo)
        {
            int hospitalRoomId = GetLoginRoomId(habbo);
            if (hospitalRoomId <= 0)
                return;

            if (habbo.CurrentRoomId != hospitalRoomId)
                habbo.PrepareRoom(hospitalRoomId, string.Empty);
            else
                PlaceOnHospitalBed(habbo);
        }

        private void PlaceOnHospitalBed(Habbo habbo)
        {
            Room room = habbo.CurrentRoom;
            if (room == null || room.RoomId != GetHospitalRoomId())
                return;

            RoomUser roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (roomUser == null)
                return;

            var beds = room.GetRoomItemHandler().GetFloor
                .Where(item => item?.GetBaseItem()?.InteractionType == InteractionType.Bed)
                .ToList();

            if (beds.Count == 0)
                return;

            var freeBeds = beds.Where(item => CanUseHospitalBed(room, roomUser, item)).ToList();
            var bedPool = freeBeds.Count > 0 ? freeBeds : beds;
            Item bed = bedPool[PlusEnvironment.GetRandomNumber(0, bedPool.Count - 1)];
            if (bed == null)
                return;

            roomUser.ClearMovement(true);
            room.GetGameMap().TeleportToItem(roomUser, bed);
            roomUser.RemoveStatus("sit");
            roomUser.SetStatus("lay", Plus.Utilities.TextHandling.GetString(bed.EffectiveHeight) + " null");
            roomUser.IsLying = true;
            roomUser.IsSitting = false;
            roomUser.Z = bed.GetZ;
            roomUser.RotHead = bed.Rotation;
            roomUser.RotBody = bed.Rotation;
            roomUser.UpdateNeeded = true;
            room.GetRoomUserManager().UpdateUserStatus(roomUser, false);
        }

        private static bool CanUseHospitalBed(Room room, RoomUser targetUser, Item bed)
        {
            RoomUser occupant = room.GetRoomUserManager().GetUserForSquare(bed.GetX, bed.GetY);
            return occupant == null || occupant.VirtualId == targetUser.VirtualId;
        }

        private static void Discharge(Habbo habbo)
        {
            Room room = habbo.CurrentRoom;
            if (room == null)
                return;

            RoomUser roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (roomUser == null)
                return;

            roomUser.ClearMovement(true);
            roomUser.RemoveStatus("lay");
            roomUser.RemoveStatus("sit");
            roomUser.IsLying = false;
            roomUser.IsSitting = false;

            Point exitTile = GetNearbyWalkableTile(room, roomUser.X, roomUser.Y) ?? room.GetGameMap().GetRandomWalkableSquare();
            room.GetGameMap().GameMap[roomUser.X, roomUser.Y] = roomUser.SqState;
            room.GetGameMap().UpdateUserMovement(new Point(roomUser.X, roomUser.Y), exitTile, roomUser);
            roomUser.SqState = room.GetGameMap().GameMap[exitTile.X, exitTile.Y];
            room.GetGameMap().GameMap[exitTile.X, exitTile.Y] = 1;
            roomUser.SetPos(exitTile.X, exitTile.Y, room.GetGameMap().Model.SqFloorHeight[exitTile.X, exitTile.Y]);
            roomUser.UpdateNeeded = true;
            room.GetRoomUserManager().UpdateUserStatus(roomUser, false);
        }

        private static Point? GetNearbyWalkableTile(Room room, int x, int y)
        {
            for (int dx = -1; dx <= 1; dx++) {
                for (int dy = -1; dy <= 1; dy++) {
                    if (dx == 0 && dy == 0)
                        continue;

                    int tileX = x + dx;
                    int tileY = y + dy;
                    if (!room.GetGameMap().ValidTile(tileX, tileY))
                        continue;

                    if (!room.GetGameMap().CanWalk(tileX, tileY, false))
                        continue;

                    if (!room.GetGameMap().SquareIsOpen(tileX, tileY, false))
                        continue;

                    return new Point(tileX, tileY);
                }
            }

            return null;
        }
    }
}