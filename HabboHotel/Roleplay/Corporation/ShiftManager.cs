using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Plus.HabboHotel.Roleplay.Corporation
{
    public class ShiftManager
    {
        public readonly ConcurrentDictionary<int, ActiveShift> _activeShifts = new();

        private readonly ConcurrentDictionary<(int, int), (int weekly, int total)> _countCache = new();

        private readonly ConcurrentDictionary<int, List<(int groupId, int credits, DateTime finished)>> _pendingLogs = new();

        public bool IsWorking(int userId) => _activeShifts.ContainsKey(userId);

        /// <summary>True when the user is currently on shift for the given corporation.</summary>
        public bool IsWorkingFor(int userId, int groupId) =>
            _activeShifts.TryGetValue(userId, out ActiveShift shift) && shift.GroupId == groupId;

        /// <summary>The corporation id the user is currently working for, or 0 when off shift.</summary>
        public int GetWorkingGroupId(int userId) =>
            _activeShifts.TryGetValue(userId, out ActiveShift shift) ? shift.GroupId : 0;

        /// <summary>Every user currently on shift for the given corporation.</summary>
        public IEnumerable<int> GetWorkersForGroup(int groupId)
        {
            foreach (var kv in _activeShifts)
                if (kv.Value.GroupId == groupId)
                    yield return kv.Key;
        }

        public void TryStartWork(GameClient session, Room room)
        {
            if (session?.GetHabbo() == null || room?.Group == null)
                return;

            Habbo habbo = session.GetHabbo();
            Group group = room.Group;

            if (habbo.GetRpStats().Energy < 1) {
                session.SendWhisper("You don't have enough energy!", 1);
                return;
            }

            if (!GroupManager.IsWorkableKind(group.Kind)) {
                session.SendWhisper("This command can only be used in corporation room!", 1);
                return;
            }

            if (!group.IsMember(habbo.Id)) {
                session.SendWhisper("You don't work here! Feel free to apply to join the corporation.", 1);
                return;
            }

            if (!group.TryGetRoleData(habbo.Id, out GroupRole role)) {
                session.SendWhisper("You don't have a role in this corporation! Contact your superior to get assigned a role.", 1);
                return;
            }

            if (IsWorking(habbo.Id)) {
                session.SendWhisper("You are already working!", 1);
                return;
            }

            //session.SendWhisper("Your role is: " + role.Name + ". Motto: " + role.ShiftMotto + ". Look " + role.ShiftCostume + ". Shift length: " + role.ShiftDuration + " minutes.", 1);

            ApplyShiftAppearance(habbo, role);

            // Going on shift forcibly unequips any weapon (staff/corp policy).
            habbo.GetRpWeapons()?.SetActiveWeaponId(0);

            _activeShifts[habbo.Id] = new ActiveShift(habbo.Id, group.Id, role.ShiftPay, role.ShiftDuration);

            // Refresh the avatar effect (e.g. apply the staff effect, drop the weapon effect).
            RpEffectService.Refresh(habbo);

            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*" + habbo.Username + " starts work as " + role.Name + "*", 0, session.GetHabbo().CustomBubbleId));

            // Update the user regardless of whether costume/motto changed.
            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (user != null)
                room.SendPacket(new UserChangeComposer(user, false));
        }

        public void TryStopWork(GameClient session, Room room)
        {
            if (session?.GetHabbo() == null || room?.Group == null)
                return;

            Habbo habbo = session.GetHabbo();
            Group group = room.Group;

            if (!GroupManager.IsWorkableKind(group.Kind)) {
                session.SendWhisper("This command can only be used in corporation room!", 1);
                return;
            }

            if (!group.IsMember(habbo.Id)) {
                session.SendWhisper("You don't work here! Feel free to apply to join the corporation.", 1);
                return;
            }

            if (!group.TryGetRoleData(habbo.Id, out GroupRole _)) {
                session.SendWhisper("You don't have a role in this corporation! Contact your superior to get assigned a role.", 1);
                return;
            }

            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*" + habbo.Username + " stops working*", 0, session.GetHabbo().CustomBubbleId));
            InterruptShift(habbo, room);
        }

        private static void ApplyShiftAppearance(Habbo habbo, GroupRole role)
        {
            // Always snapshot the current motto/look first — even when the role has no costume
            // or motto — so stopping work restores exactly what the user was wearing rather
            // than a stale value left over from a previous shift.
            habbo.OldMotto = habbo.Motto;
            habbo.OldLook = habbo.Look;

            if (!string.IsNullOrEmpty(role.ShiftMotto))
                habbo.Motto = role.ShiftMotto;

            if (string.IsNullOrEmpty(role.ShiftCostume))
                return;

            string gender = habbo.Gender.ToLower();
            string figureString = role.ShiftCostume;
            string selectedFigure = null;

            if (figureString.Contains('|')) {
                Dictionary<string, string> figures = new();
                foreach (string part in figureString.Split(';'))
                {
                    if (string.IsNullOrWhiteSpace(part))
                        continue;

                    string[] split = part.Split('|');
                    if (split.Length != 2)
                        continue;

                    figures[split[0].ToLower()] = split[1];
                }

                if (figures.TryGetValue(gender, out string fig))
                    selectedFigure = fig;
                else if (figures.TryGetValue("m", out fig))
                    selectedFigure = fig;
            } else {
                selectedFigure = figureString;
            }

            if (string.IsNullOrWhiteSpace(selectedFigure))
                return;

            Dictionary<string, string> lookParts = new();

            foreach (string part in habbo.Look.Split('.'))
            {
                if (string.IsNullOrWhiteSpace(part))
                    continue;

                string[] split = part.Split('-');
                if (split.Length == 0)
                    continue;

                lookParts[split[0]] = part;
            }

            foreach (string part in selectedFigure.Split('.'))
            {
                if (string.IsNullOrWhiteSpace(part))
                    continue;

                string[] split = part.Split('-');
                if (split.Length == 0)
                    continue;

                lookParts[split[0]] = part;
            }

            habbo.Look = string.Join(".", lookParts.Values);
        }

        public void Tick(Habbo habbo)
        {
            if (!_activeShifts.TryGetValue(habbo.Id, out ActiveShift shift))
                return;

            shift.TicksRemaining--;
            if (shift.TicksRemaining > 0)
                return;

            // Full cycle complete: pay ShiftPay + accumulated bonus, then restart
            int pay = shift.ShiftPay + shift.BonusCredits;
            GivePay(habbo, pay);
            AddPendingLog(habbo.Id, shift.GroupId, pay);
            IncrementCountCache(shift.GroupId, habbo.Id);

            // Restart cycle (no costume/motto change needed — already wearing it)
            _activeShifts[habbo.Id] = new ActiveShift(habbo.Id, shift.GroupId, shift.ShiftPay, shift.ShiftDurationMinutes);
        }

        public void InterruptShift(Habbo habbo, Room room)
        {
            if (!_activeShifts.TryRemove(habbo.Id, out ActiveShift shift))
                return;

            if (shift.BonusCredits > 0) {
                GivePay(habbo, shift.BonusCredits);
                AddPendingLog(habbo.Id, shift.GroupId, shift.BonusCredits);
                IncrementCountCache(shift.GroupId, habbo.Id);
            }

            RestoreAppearance(habbo, room);

            // Leaving the shift drops effect (falls back to weapon/passive/none).
            RpEffectService.Refresh(habbo);
        }

        /// <summary>
        /// Called when a working user leaves the room they're on shift in (summon, follow,
        /// or simply walking out). The shift ends unless the user's corporation role carries
        /// the `room_swap` permission, which lets that role roam between rooms while on shift.
        /// </summary>
        public void HandleRoomExit(Habbo habbo, Room room)
        {
            if (habbo == null || !_activeShifts.TryGetValue(habbo.Id, out ActiveShift shift))
                return;

            if (PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(shift.GroupId, out Group group)
                && group.HasPermission(habbo.Id, "room_swap"))
                return;

            habbo.GetClient()?.SendWhisper("You stopped working because you left the workplace.", 1);
            InterruptShift(habbo, room);
        }

        public void OnDisconnect(Habbo habbo)
        {
            if (_activeShifts.TryRemove(habbo.Id, out ActiveShift shift)) {
                if (shift.BonusCredits > 0) {
                    habbo.Credits += shift.BonusCredits;
                    AddPendingLog(habbo.Id, shift.GroupId, shift.BonusCredits);
                    IncrementCountCache(shift.GroupId, habbo.Id);
                }

                habbo.Motto = habbo.OldMotto;
                habbo.Look = habbo.OldLook;
            }

            FlushLogsToDatabase(habbo.Id);
        }

        public void AddBonusCredits(int userId, int amount)
        {
            if (_activeShifts.TryGetValue(userId, out ActiveShift shift))
                shift.AddBonus(amount);
        }

        public (int weekly, int total) GetShiftCounts(int groupId, int userId)
        {
            var key = (groupId, userId);
            if (!_countCache.ContainsKey(key))
                LoadCountsFromDatabase(groupId, userId);

            return _countCache.TryGetValue(key, out var counts) ? counts : (0, 0);
        }

        private static void GivePay(Habbo habbo, int amount)
        {
            habbo.Credits += amount;
            habbo.GetClient()?.SendPacket(new CreditBalanceComposer(habbo.Credits));
        }

        private static void RestoreAppearance(Habbo habbo, Room room)
        {
            if (habbo.OldMotto != null)
                habbo.Motto = habbo.OldMotto;
            if (habbo.OldLook != null)
                habbo.Look = habbo.OldLook;

            if (room == null) return;
            RoomUser roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (roomUser != null)
                room.SendPacket(new UserChangeComposer(roomUser, false));
        }

        private void AddPendingLog(int userId, int groupId, int credits)
        {
            var logs = _pendingLogs.GetOrAdd(userId, _ => new List<(int, int, DateTime)>());
            lock (logs)
                logs.Add((groupId, credits, DateTime.Now));
        }

        private void IncrementCountCache(int groupId, int userId)
        {
            var key = (groupId, userId);

            if (!_countCache.ContainsKey(key))
                LoadCountsFromDatabase(groupId, userId);

            _countCache.AddOrUpdate(key,
                _ => (IsCurrentWeek(DateTime.Now) ? 1 : 0, 1),
                (_, old) => (IsCurrentWeek(DateTime.Now) ? old.weekly + 1 : old.weekly, old.total + 1));
        }

        private void LoadCountsFromDatabase(int groupId, int userId)
        {
            int total = 0, weekly = 0;
            try
            {
                using WavePlusContext db = PlusEnvironment.GetDbContext();
                total = db.rp_shift_logs.Count(x => x.group_id == groupId && x.user_id == userId);

                // YEARWEEK(..,1)=YEARWEEK(NOW(),1) is the ISO current-week test; EF can't
                // translate YEARWEEK, so pull this user's shift-finish times and count the
                // ones in the current ISO week in memory (IsCurrentWeek uses ISOWeek).
                weekly = db.rp_shift_logs
                    .Where(x => x.group_id == groupId && x.user_id == userId)
                    .Select(x => x.shift_finished)
                    .ToList()
                    .Count(IsCurrentWeek);
            }
            catch { }

            _countCache[(groupId, userId)] = (weekly, total);
        }

        private void FlushLogsToDatabase(int userId)
        {
            if (!_pendingLogs.TryRemove(userId, out var logs))
                return;

            List<(int groupId, int credits, DateTime finished)> snapshot;
            lock (logs)
                snapshot = new List<(int, int, DateTime)>(logs);

            if (snapshot.Count == 0)
                return;

            try
            {
                using WavePlusContext db = PlusEnvironment.GetDbContext();
                foreach (var (groupId, credits, finished) in snapshot)
                {
                    db.rp_shift_logs.Add(new rp_shift_log
                    {
                        group_id = groupId,
                        user_id = userId,
                        credits_paid = credits,
                        shift_finished = finished
                    });
                }
                db.SaveChanges();
            }
            catch { }
        }

        private static bool IsCurrentWeek(DateTime dt)
        {
            int dtWeek = ISOWeek.GetWeekOfYear(dt);
            int nowWeek = ISOWeek.GetWeekOfYear(DateTime.Now);
            return dt.Year == DateTime.Now.Year && dtWeek == nowWeek;
        }
    }
}
