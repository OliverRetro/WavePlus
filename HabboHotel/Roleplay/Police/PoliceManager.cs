using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Roleplay;

namespace Plus.HabboHotel.Roleplay.Police
{
    public sealed class PoliceManager
    {
        private readonly ConcurrentDictionary<int, PoliceState> _states = new();

        public bool IsCuffed(int userId) =>
            _states.TryGetValue(userId, out PoliceState state) && state.Cuffed;

        public bool IsEscorted(int userId) =>
            _states.TryGetValue(userId, out PoliceState state) && state.EscortedBy > 0;

        public bool IsFrozen(int userId) =>
            _states.TryGetValue(userId, out PoliceState state) && state.Frozen;

        public int GetEscortingOfficer(int userId) =>
            _states.TryGetValue(userId, out PoliceState state) ? state.EscortedBy : 0;

        public string GetOriginalLook(int userId) =>
            _states.TryGetValue(userId, out PoliceState state) && !string.IsNullOrEmpty(state.OriginalLook)
                ? state.OriginalLook
                : null;

        public void Cuff(Habbo officer, Habbo target)
        {
            if (officer == null || target == null)
                return;

            PoliceState state = _states.GetOrAdd(target.Id, _ => new PoliceState());
            state.Cuffed = true;
            state.Frozen = false;
            state.CuffedBy = officer.Id;
            if (string.IsNullOrEmpty(state.OriginalLook))
                state.OriginalLook = target.Look;

            RpEffectService.Refresh(target);
            Persist(target, state);
        }

        public void RestoreCuffedFree(Habbo target, string originalLook)
        {
            if (target == null || _states.ContainsKey(target.Id))
                return;

            PoliceState state = _states.GetOrAdd(target.Id, _ => new PoliceState());
            state.Cuffed = true;
            state.Frozen = false;
            state.OriginalLook = string.IsNullOrEmpty(originalLook) ? target.Look : originalLook;

            RpEffectService.Refresh(target);
        }

        public void HandleRoomEntry(Habbo habbo)
        {
            UserRpStats stats = habbo?.GetRpStats();
            if (stats == null || !stats.IsCuffed || _states.ContainsKey(habbo.Id))
                return;

            // A jailed suspect is cuffed by the JailManager instead.
            if (stats.JailPending || stats.JailTimeLeft > 0 || stats.JailReleaseTime > PlusEnvironment.GetUnixTimestamp())
                return;

            RestoreCuffedFree(habbo, stats.JailRevertLook);
        }

        public void EnterJailCuff(Habbo target, string originalLook)
        {
            if (target == null)
                return;

            PoliceState state = _states.GetOrAdd(target.Id, _ => new PoliceState());
            state.Cuffed = true;
            state.Frozen = false;
            state.EscortedBy = 0;
            if (string.IsNullOrEmpty(state.OriginalLook))
                state.OriginalLook = string.IsNullOrEmpty(originalLook) ? target.Look : originalLook;

            ApplyFastwalk(target, false);
            ApplyFrozen(target, false);
            // Now jailed — drop the cuffs effect so it doesn't double up on the jail outfit.
            RpEffectService.Refresh(target);
            Persist(target, state);
        }

        public void Uncuff(Habbo target)
        {
            if (target == null)
                return;

            if (_states.TryRemove(target.Id, out PoliceState state)) {
                EndEscortMovement(target);
                RestoreLook(target, state.OriginalLook);
                ApplyFastwalk(target, false);
                ApplyFrozen(target, false);
                // State's gone, so this resolves to whatever they'd normally show (drops the cuffs).
                RpEffectService.Refresh(target);
                ClearCuffPersistence(target);
            }
        }

        public void BreakFree(Habbo target)
        {
            // Only counts as an escape if they were actually in custody (cuffed/being escorted).
            bool wasInCustody = target != null && _states.ContainsKey(target.Id);
            RemoveCuffs(target, wipeCrimes: true);

            if (wasInCustody)
                PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(target.GetClient(), "ACH_Escaper", 1);
        }

        private void RemoveCuffs(Habbo target, bool wipeCrimes)
        {
            if (target == null)
                return;

            _states.TryRemove(target.Id, out PoliceState state);
            EndEscortMovement(target);
            RestoreLook(target, state?.OriginalLook);
            ApplyFastwalk(target, false);
            ApplyFrozen(target, false);
            // State's gone, so this resolves to whatever they'd normally show (drops the cuffs).
            RpEffectService.Refresh(target);

            PlusEnvironment.GetJailManager().Forget(target);
            if (wipeCrimes)
                PlusEnvironment.GetRpCrimeManager().ResolveAll(target.Id);
            ClearCuffPersistence(target);
        }

        public void StartArrestEscort(Habbo officer, Habbo target, int pendingStars)
        {
            if (officer == null || target == null)
                return;

            PoliceState state = _states.GetOrAdd(target.Id, _ => new PoliceState { Cuffed = true, CuffedBy = officer.Id, OriginalLook = target.Look });
            state.Cuffed = true;
            state.Frozen = true;
            state.EscortedBy = officer.Id;
            state.PendingStars = pendingStars;
            CaptureEscortFormation(officer, target, state);
            ApplyFrozen(target, true);
            Persist(target, state);
        }

        public void StopEscort(int targetId)
        {
            if (_states.TryGetValue(targetId, out PoliceState state)) {
                state.EscortedBy = 0;
                state.Frozen = false;
                GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(targetId);
                if (client?.GetHabbo() != null) {
                    EndEscortMovement(client.GetHabbo());
                    ApplyFastwalk(client.GetHabbo(), false);
                    ApplyFrozen(client.GetHabbo(), false);
                }
            }
        }

        public void OnDisconnect(Habbo habbo)
        {
            if (habbo == null)
                return;

            if (_states.TryRemove(habbo.Id, out PoliceState ownState)) {
                UserRpStats stats = habbo.GetRpStats();
                if (stats != null) {
                    stats.IsCuffed = ownState.Cuffed;
                    stats.JailRevertLook = ownState.OriginalLook ?? habbo.Look;

                    // Disconnected while being escorted — jail them on relog.
                    if (ownState.EscortedBy > 0) {
                        stats.JailPending = true;
                        stats.JailStars = ownState.PendingStars;
                    }
                    habbo.SaveRpStats();
                }
            }

            FreeControlledSuspects(habbo.Id, "Your escorting officer left — you can move, but you're still cuffed.");
        }

        public void OnDeath(Habbo habbo)
        {
            if (habbo == null)
                return;

            FreeControlledSuspects(habbo.Id, "Your escorting officer is down — you can move, but you're still cuffed.");

            // Being killed knocks the cuffs off (but doesn't wipe what you're wanted for).
            if (_states.ContainsKey(habbo.Id))
                RemoveCuffs(habbo, wipeCrimes: false);
        }

        public void OnCycle()
        {
            HashSet<int> jailRooms = GetJailRoomIds();

            foreach (var kv in _states) {
                int targetId = kv.Key;
                PoliceState state = kv.Value;

                if (state.EscortedBy <= 0)
                    continue;

                GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(targetId);
                Habbo target = targetClient?.GetHabbo();

                // Don't remove the state here: the client-manager lookup can fail mid-disconnect
                // before OnDisconnect has persisted the cuffs, and removing the state first would
                // silently release the suspect. OnDisconnect owns the cleanup.
                if (target == null)
                    continue;

                GameClient officerClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(state.EscortedBy);
                Habbo officer = officerClient?.GetHabbo();

                if (officer == null || officer.CurrentRoom == null) {
                    // Officer vanished without a clean disconnect — free but keep cuffed.
                    state.EscortedBy = 0;
                    state.Frozen = false;
                    EndEscortMovement(target);
                    ApplyFastwalk(target, false);
                    ApplyFrozen(target, false);
                    continue;
                }

                // Follow the officer across rooms — mirror his room swaps too. SyncEscorts snaps
                // the suspect back into formation once they've loaded into the officer's room.
                if (officer.CurrentRoomId != target.CurrentRoomId) {
                    target.PrepareRoom(officer.CurrentRoomId, string.Empty);
                    continue;
                }

                // Reached a jail room — jail the suspect and end the escort.
                if (jailRooms.Contains(officer.CurrentRoomId)) {
                    if (PlusEnvironment.GetJailManager().JailFromEscort(target, officer, state.PendingStars, state.OriginalLook)) {
                        state.EscortedBy = 0;
                        state.Frozen = false;
                        state.PendingStars = 0;
                        EndEscortMovement(target);
                        ApplyFastwalk(target, false);
                    }
                }
            }
        }

        public void SyncEscorts(Room room)
        {
            if (room == null || _states.IsEmpty)
                return;

            foreach (var kv in _states) {
                PoliceState state = kv.Value;
                if (state.EscortedBy <= 0)
                    continue;

                RoomUser officerUser = room.GetRoomUserManager().GetRoomUserByHabbo(state.EscortedBy);
                if (officerUser == null)
                    continue; // officer isn't in this room (cross-room follow is handled in OnCycle)

                RoomUser suspect = room.GetRoomUserManager().GetRoomUserByHabbo(kv.Key);
                if (suspect == null)
                    continue;

                int rot = officerUser.RotBody;

                // The officer's authoritative tile this tick: the tile he's stepping onto while
                // walking, otherwise where he stands.
                bool officerWalking = officerUser.IsWalking || officerUser.Statusses.ContainsKey("mv");
                int ox = officerWalking ? officerUser.SetX : officerUser.X;
                int oy = officerWalking ? officerUser.SetY : officerUser.Y;

                bool sameRoom = state.LastOfficerRoomId == room.RoomId;
                bool firstSight = state.LastOfficerX == int.MinValue;

                // Officer hasn't moved since we last mirrored — keep facings synced and let the
                // suspect's last step finish, but do NOT re-issue it. Re-issuing every 50ms tick
                // (the old bug) reset the step's pacing forever, so it never committed until the
                // officer stopped — the stutter-then-teleport the escort used to show.
                if (sameRoom && !firstSight && ox == state.LastOfficerX && oy == state.LastOfficerY) {
                    SettleSuspect(suspect, rot);
                    continue;
                }

                int stepDist = firstSight || !sameRoom
                    ? int.MaxValue
                    : Chebyshev(state.LastOfficerX, state.LastOfficerY, ox, oy);
                bool roomChanged = !sameRoom;

                state.LastOfficerX = ox;
                state.LastOfficerY = oy;
                state.LastOfficerRoomId = room.RoomId;

                // Where the suspect should be: officer tile + held offset (its meaning is gone
                // across a room swap, so re-form beside him there).
                Point desired = roomChanged
                    ? ReformNextToOfficer(room, ox, oy, rot, state)
                    : ResolveSuspectTile(room, ox, oy, rot, state);

                if (suspect.X == desired.X && suspect.Y == desired.Y) {
                    SettleSuspect(suspect, rot);
                    continue;
                }

                // A walk step is mirrored as an animated step; anything larger — a teleport, a room
                // swap, first placement, or a drift bigger than a fast-walk hop — snaps instantly.
                bool teleport = roomChanged || firstSight || !officerWalking || stepDist > 2;
                if (teleport)
                    SnapTo(room, suspect, desired.X, desired.Y, rot);
                else
                    IssueMirrorStep(room, suspect, desired.X, desired.Y, rot);
            }
        }

        private static void CaptureEscortFormation(Habbo officer, Habbo target, PoliceState state)
        {
            state.HasOffset = false;
            state.LastOfficerX = int.MinValue;
            state.LastOfficerY = int.MinValue;
            state.LastOfficerRoomId = 0;

            Room room = officer.CurrentRoom;
            if (room == null || officer.CurrentRoomId != target.CurrentRoomId)
                return;

            RoomUser officerUser = room.GetRoomUserManager().GetRoomUserByHabbo(officer.Id);
            RoomUser suspectUser = room.GetRoomUserManager().GetRoomUserByHabbo(target.Id);
            if (officerUser == null || suspectUser == null)
                return;

            state.OffsetX = suspectUser.X - officerUser.X;
            state.OffsetY = suspectUser.Y - officerUser.Y;
            state.HasOffset = true;
            state.LastOfficerX = officerUser.X;
            state.LastOfficerY = officerUser.Y;
            state.LastOfficerRoomId = room.RoomId;
        }

        private static Point ResolveSuspectTile(Room room, int ox, int oy, int rot, PoliceState state)
        {
            if (state.HasOffset) {
                int cx = ox + state.OffsetX;
                int cy = oy + state.OffsetY;
                if (room.GetGameMap().ValidTile(cx, cy))
                    return new Point(cx, cy);
            }

            Point front = SquareInDirection(ox, oy, rot);
            if (room.GetGameMap().ValidTile(front.X, front.Y))
                return front;

            return new Point(ox, oy);
        }

        private static Point ReformNextToOfficer(Room room, int ox, int oy, int rot, PoliceState state)
        {
            Point tile = FindHoldTile(room, ox, oy, rot);
            state.OffsetX = tile.X - ox;
            state.OffsetY = tile.Y - oy;
            state.HasOffset = true;
            return tile;
        }

        private static Point FindHoldTile(Room room, int ox, int oy, int rot)
        {
            Point front = SquareInDirection(ox, oy, rot);
            if (room.GetGameMap().ValidTile(front.X, front.Y))
                return front;

            for (int r = 0; r < 8; r++) {
                Point p = SquareInDirection(ox, oy, r);
                if (room.GetGameMap().ValidTile(p.X, p.Y))
                    return p;
            }

            return new Point(ox, oy);
        }

        private static void IssueMirrorStep(Room room, RoomUser suspect, int x, int y, int rot)
        {
            double stepZ = room.GetGameMap().SqAbsoluteHeight(x, y);

            suspect.Statusses.Remove("lay");
            suspect.Statusses.Remove("sit");
            suspect.RemoveStatus("mv");
            suspect.SetStatus("mv", x + "," + y + "," + Plus.Utilities.TextHandling.GetString(stepZ));
            suspect.RotBody = rot;
            suspect.RotHead = rot;
            suspect.AllowOverride = true; // the fused step must always land, even on "blocked" tiles
            suspect.SetStep = true;
            suspect.SetX = x;
            suspect.SetY = y;
            suspect.SetZ = stepZ;
            // Pace the fused step like a real one: without this the 50ms movement tick would commit
            // it (and strip the "mv") almost immediately, cutting the walk animation short.
            suspect.NextStepMs = PlusEnvironment.MonotonicMs() + suspect.StepDurationMs;
            suspect.UpdateNeeded = true;
        }

        private static void SettleSuspect(RoomUser suspect, int rot)
        {
            bool changed = false;

            if (suspect.RotBody != rot || suspect.RotHead != rot) {
                suspect.RotBody = rot;
                suspect.RotHead = rot;
                changed = true;
            }

            if (!suspect.SetStep && suspect.Statusses.ContainsKey("mv")) {
                suspect.RemoveStatus("mv");
                changed = true;
            }

            if (changed)
                suspect.UpdateNeeded = true;
        }

        private static int Chebyshev(int x1, int y1, int x2, int y2) =>
            System.Math.Max(System.Math.Abs(x1 - x2), System.Math.Abs(y1 - y2));

        private static Point SquareInDirection(int x, int y, int rot)
        {
            return rot switch
            {
                0 => new Point(x, y - 1),
                1 => new Point(x + 1, y - 1),
                2 => new Point(x + 1, y),
                3 => new Point(x + 1, y + 1),
                4 => new Point(x, y + 1),
                5 => new Point(x - 1, y + 1),
                6 => new Point(x - 1, y),
                7 => new Point(x - 1, y - 1),
                _ => new Point(x, y)
            };
        }

        private static void SnapTo(Room room, RoomUser roomUser, int x, int y, int rot)
        {
            roomUser.ClearMovement(true);
            roomUser.Statusses.Remove("lay");
            roomUser.Statusses.Remove("sit");
            room.GetGameMap().UpdateUserMovement(new Point(roomUser.X, roomUser.Y), new Point(x, y), roomUser);
            roomUser.SetPos(x, y, room.GetGameMap().SqAbsoluteHeight(x, y));
            roomUser.RotBody = rot;
            roomUser.RotHead = rot;
            roomUser.UpdateNeeded = true;
        }

        private void FreeControlledSuspects(int officerId, string message)
        {
            foreach (var kv in _states) {
                PoliceState state = kv.Value;
                if (state.EscortedBy != officerId && state.CuffedBy != officerId)
                    continue;

                bool wasControlled = state.EscortedBy == officerId || state.Frozen;
                state.EscortedBy = 0;
                state.Frozen = false;

                GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(kv.Key);
                if (client?.GetHabbo() != null) {
                    EndEscortMovement(client.GetHabbo());
                    ApplyFastwalk(client.GetHabbo(), false);
                    ApplyFrozen(client.GetHabbo(), false);
                    if (wasControlled)
                        client.SendWhisper(message, 1);
                }
            }
        }

        public static HashSet<int> GetJailRoomIds()
        {
            HashSet<int> rooms = new();

            string raw = PlusEnvironment.GetSettingsManager().TryGetValue("rp.jail.rooms");
            if (!string.IsNullOrWhiteSpace(raw)) {
                foreach (string part in raw.Split(';'))
                    if (int.TryParse(part.Trim(), out int id) && id > 0)
                        rooms.Add(id);
            }

            int primary = PlusEnvironment.GetJailManager().GetJailRoomId();
            if (primary > 0)
                rooms.Add(primary);

            var spawn = PlusEnvironment.GetJailManager().GetJailSpawn();
            if (spawn != null && spawn.roomid > 0)
                rooms.Add(spawn.roomid);

            return rooms;
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

        public static int GetConfiguredGroupId(string settingKey)
        {
            return int.TryParse(PlusEnvironment.GetSettingsManager().TryGetValue(settingKey), out int id) ? id : 0;
        }

        private static void Persist(Habbo target, PoliceState state)
        {
            UserRpStats stats = target.GetRpStats();
            if (stats == null)
                return;

            stats.IsCuffed = true;
            stats.JailRevertLook = state.OriginalLook ?? target.Look;
            target.SaveRpStats();
        }

        private static void ClearCuffPersistence(Habbo target)
        {
            UserRpStats stats = target.GetRpStats();
            if (stats == null)
                return;

            stats.IsCuffed = false;
            stats.JailRevertLook = "";
            target.SaveRpStats();
        }

        private static void ApplyFrozen(Habbo target, bool frozen)
        {
            RoomUser roomUser = target?.CurrentRoom?.GetRoomUserManager().GetRoomUserByHabbo(target.Id);
            if (roomUser == null)
                return;

            roomUser.CanWalk = !frozen;
            if (frozen)
                roomUser.ClearMovement(true);
            roomUser.UpdateNeeded = true;
        }

        private static void ApplyFastwalk(Habbo target, bool enabled)
        {
            RoomUser roomUser = target?.CurrentRoom?.GetRoomUserManager().GetRoomUserByHabbo(target.Id);
            if (roomUser != null) {
                roomUser.FastWalking = enabled;
                if (!enabled)
                    roomUser.AllowOverride = false; // drop the escort-lockstep override too
            }
        }

        private static void EndEscortMovement(Habbo target)
        {
            RoomUser roomUser = target?.CurrentRoom?.GetRoomUserManager().GetRoomUserByHabbo(target.Id);
            if (roomUser == null)
                return;

            roomUser.RemoveStatus("mv");
            roomUser.SetStep = false;
            roomUser.AllowOverride = false;
            roomUser.UpdateNeeded = true;
        }

        private static void RestoreLook(Habbo target, string originalLook)
        {
            if (string.IsNullOrEmpty(originalLook))
                return;

            target.Look = originalLook;
            BroadcastLook(target);
        }

        private static void BroadcastLook(Habbo target)
        {
            Room room = target.CurrentRoom;
            RoomUser roomUser = room?.GetRoomUserManager().GetRoomUserByHabbo(target.Id);
            if (roomUser != null)
                room.SendPacket(new UserChangeComposer(roomUser, false));
        }

        private sealed class PoliceState
        {
            public bool Cuffed;
            public bool Frozen;
            public int CuffedBy;
            public string OriginalLook;
            public int EscortedBy;
            public int PendingStars;

            // Escort formation. The suspect holds OffsetX/OffsetY tiles from the officer and mirrors
            // his movement as a rigid pair. LastOfficerX/Y/RoomId is the officer tile we last acted
            // on, so a single step issues one mirror step instead of one per 50ms tick.
            public int OffsetX;
            public int OffsetY;
            public bool HasOffset;
            public int LastOfficerX = int.MinValue;
            public int LastOfficerY = int.MinValue;
            public int LastOfficerRoomId;
        }
    }
}