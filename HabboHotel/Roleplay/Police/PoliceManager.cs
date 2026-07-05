using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Roleplay.Police
{
    /// <summary>
    /// In-memory police state for cuffs and escorts. It is intentionally not persisted:
    /// reconnecting clears cuffs/escorts, while crimes/jail are handled by RpCrimeManager/JailManager.
    /// </summary>
    public sealed class PoliceManager
    {
        private readonly ConcurrentDictionary<int, PoliceState> _states = new();

        public bool IsCuffed(int userId) =>
            _states.TryGetValue(userId, out PoliceState state) && state.Cuffed;

        public bool IsEscorted(int userId) =>
            _states.TryGetValue(userId, out PoliceState state) && state.EscortedBy > 0;

        public int GetEscortingOfficer(int userId) =>
            _states.TryGetValue(userId, out PoliceState state) ? state.EscortedBy : 0;

        public void Cuff(Habbo officer, Habbo target)
        {
            if (officer == null || target == null)
                return;

            PoliceState state = _states.GetOrAdd(target.Id, _ => new PoliceState());
            state.Cuffed = true;
            state.CuffedBy = officer.Id;
            ApplyCuffedState(target, true);
        }

        public void Uncuff(Habbo target)
        {
            if (target == null)
                return;

            if (_states.TryRemove(target.Id, out _))
                ApplyCuffedState(target, false);
        }

        public void StartEscort(Habbo officer, Habbo target)
        {
            if (officer == null || target == null)
                return;

            PoliceState state = _states.GetOrAdd(target.Id, _ => new PoliceState { Cuffed = true, CuffedBy = officer.Id });
            state.Cuffed = true;
            state.EscortedBy = officer.Id;
            ApplyCuffedState(target, true);
        }

        public void StopEscort(int targetId)
        {
            if (_states.TryGetValue(targetId, out PoliceState state))
                state.EscortedBy = 0;
        }

        public void Clear(int userId)
        {
            if (_states.TryRemove(userId, out _)) {
                GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
                if (client?.GetHabbo() != null)
                    ApplyCuffedState(client.GetHabbo(), false);
            }

            foreach (var kv in _states)
            {
                if (kv.Value.CuffedBy != userId && kv.Value.EscortedBy != userId)
                    continue;

                if (_states.TryRemove(kv.Key, out _)) {
                    GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(kv.Key);
                    if (targetClient?.GetHabbo() != null)
                        ApplyCuffedState(targetClient.GetHabbo(), false);
                }
            }
        }

        public void OnCycle()
        {
            foreach (var kv in _states)
            {
                int targetId = kv.Key;
                PoliceState state = kv.Value;

                GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(targetId);
                Habbo target = targetClient?.GetHabbo();

                if (target == null) {
                    _states.TryRemove(targetId, out _);
                    continue;
                }

                ApplyCuffedState(target, state.Cuffed);

                if (state.EscortedBy <= 0)
                    continue;

                GameClient officerClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(state.EscortedBy);
                Habbo officer = officerClient?.GetHabbo();

                if (officer == null || officer.CurrentRoomId != target.CurrentRoomId || officer.CurrentRoom == null) {
                    state.EscortedBy = 0;
                    continue;
                }

                Room room = officer.CurrentRoom;
                RoomUser officerUser = room.GetRoomUserManager().GetRoomUserByHabbo(officer.Id);
                RoomUser targetUser = room.GetRoomUserManager().GetRoomUserByHabbo(target.Id);
                if (officerUser == null || targetUser == null)
                    continue;

                if (Math.Abs(officerUser.X - targetUser.X) + Math.Abs(officerUser.Y - targetUser.Y) <= 1)
                    continue;

                Point? destination = GetEscortDestination(room, officerUser);
                if (destination == null)
                    continue;

                targetUser.MoveTo(destination.Value.X, destination.Value.Y, true);
            }
        }

        public static bool IsOnDutyPolice(Habbo habbo)
        {
            int groupId = GetConfiguredGroupId("rp.police.corporation.id");
            return groupId > 0 && PlusEnvironment.GetGame().GetShiftManager().IsWorkingFor(habbo.Id, groupId);
        }

        public static bool IsOnDutyAmbulance(Habbo habbo)
        {
            int groupId = GetConfiguredGroupId("rp.ambulance.corporation.id");
            return groupId > 0 && PlusEnvironment.GetGame().GetShiftManager().IsWorkingFor(habbo.Id, groupId);
        }

        public static bool HasJobPermission(Habbo habbo, string settingKey, string permissionKey)
        {
            int groupId = GetConfiguredGroupId(settingKey);
            if (groupId <= 0 || habbo == null)
                return false;

            if (!PlusEnvironment.GetGame().GetShiftManager().IsWorkingFor(habbo.Id, groupId))
                return false;

            if (string.IsNullOrWhiteSpace(permissionKey))
                return true;

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out Group group))
                return true;

            // If the permission key exists and role permissions are configured, enforce it.
            // If no assignment exists yet, on-duty membership remains enough for backwards compatibility.
            return group.IsOwnerOrHasPermission(habbo.Id, permissionKey) || group.GetPermissions.Count == 0;
        }

        public static int GetConfiguredGroupId(string settingKey)
        {
            return int.TryParse(PlusEnvironment.GetSettingsManager().TryGetValue(settingKey), out int id) ? id : 0;
        }

        private static void ApplyCuffedState(Habbo target, bool cuffed)
        {
            RoomUser roomUser = target?.CurrentRoom?.GetRoomUserManager().GetRoomUserByHabbo(target.Id);
            if (roomUser == null)
                return;

            roomUser.CanWalk = !cuffed;
            if (cuffed)
                roomUser.ClearMovement(true);
            roomUser.UpdateNeeded = true;
        }

        private static Point? GetEscortDestination(Room room, RoomUser officer)
        {
            Point[] candidates =
            {
                officer.SquareBehind,
                officer.SquareLeft,
                officer.SquareRight,
                officer.SquareInFront
            };

            foreach (Point point in candidates)
            {
                if (!room.GetGameMap().ValidTile(point.X, point.Y))
                    continue;

                if (!room.GetGameMap().CanWalk(point.X, point.Y, false))
                    continue;

                if (!room.GetGameMap().SquareIsOpen(point.X, point.Y, false))
                    continue;

                return point;
            }

            return null;
        }

        private sealed class PoliceState
        {
            public bool Cuffed;
            public int CuffedBy;
            public int EscortedBy;
        }
    }
}
