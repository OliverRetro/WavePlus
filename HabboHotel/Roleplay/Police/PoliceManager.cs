using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Roleplay;

namespace Plus.HabboHotel.Roleplay.Police
{
    /// <summary>
    /// In-memory police state for cuffs and escorts, mirrored to the suspect's rp statistics so
    /// cuffs survive relog/restart. A cuffed suspect is either <b>frozen</b> (fresh cuff or being
    /// escorted — can't move) or <b>free to move</b> (their escorting officer left/died, or they
    /// relogged). Either way the cuffs stay: they can't equip weapons, hit or rob, and can only
    /// escape via a lockpick (the `handpick` item) or by being killed. Escorting a suspect into a
    /// jail room hands them to the JailManager, which owns the jail outfit while they serve time.
    /// </summary>
    public sealed class PoliceManager
    {
        private readonly ConcurrentDictionary<int, PoliceState> _states = new();

        public bool IsCuffed(int userId) =>
            _states.TryGetValue(userId, out PoliceState state) && state.Cuffed;

        public bool IsEscorted(int userId) =>
            _states.TryGetValue(userId, out PoliceState state) && state.EscortedBy > 0;

        /// <summary>True while the suspect is held immobile (fresh cuff, escort, or jail).</summary>
        public bool IsFrozen(int userId) =>
            _states.TryGetValue(userId, out PoliceState state) && state.Frozen;

        public int GetEscortingOfficer(int userId) =>
            _states.TryGetValue(userId, out PoliceState state) ? state.EscortedBy : 0;

        public string GetOriginalLook(int userId) =>
            _states.TryGetValue(userId, out PoliceState state) && !string.IsNullOrEmpty(state.OriginalLook)
                ? state.OriginalLook
                : null;

        /// <summary>Cuffs a suspect: snapshots their look, swaps the cuff shirt and freezes them.</summary>
        public void Cuff(Habbo officer, Habbo target)
        {
            if (officer == null || target == null)
                return;

            PoliceState state = _states.GetOrAdd(target.Id, _ => new PoliceState());
            state.Cuffed = true;
            state.Frozen = true;
            state.CuffedBy = officer.Id;
            if (string.IsNullOrEmpty(state.OriginalLook))
                state.OriginalLook = target.Look;

            ApplyCuffLook(target, state.OriginalLook);
            ApplyFrozen(target, true);
            Persist(target, state);
        }

        /// <summary>
        /// Re-establishes a cuffed-but-free suspect from persistence (e.g. after they relog while
        /// still cuffed but not jailed). Cuffs are shown, restrictions apply, but they can move.
        /// </summary>
        public void RestoreCuffedFree(Habbo target, string originalLook)
        {
            if (target == null || _states.ContainsKey(target.Id))
                return;

            PoliceState state = _states.GetOrAdd(target.Id, _ => new PoliceState());
            state.Cuffed = true;
            state.Frozen = false;
            state.OriginalLook = string.IsNullOrEmpty(originalLook) ? target.Look : originalLook;

            ApplyCuffLook(target, state.OriginalLook);
        }

        /// <summary>
        /// On room entry, re-establishes cuffs for a suspect who relogged still cuffed but not
        /// jailed (the JailManager handles the jailed case). Restores them free-to-move.
        /// </summary>
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

        /// <summary>
        /// Marks a suspect as cuffed for jail, without touching their look — the JailManager owns
        /// the jail outfit while the sentence is served. Jailed suspects can move around the jail
        /// room freely (the JailManager confines them to it), so they are not frozen.
        /// </summary>
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
            Persist(target, state);
        }

        /// <summary>Removes a suspect's cuffs and restores the look they had before cuffing.</summary>
        public void Uncuff(Habbo target)
        {
            if (target == null)
                return;

            if (_states.TryRemove(target.Id, out PoliceState state)) {
                RestoreLook(target, state.OriginalLook);
                ApplyFastwalk(target, false);
                ApplyFrozen(target, false);
                ClearCuffPersistence(target);
            }
        }

        /// <summary>
        /// The suspect picks their cuffs (handpick): drops the cuffs, wipes any jail data and their
        /// crimes, and restores their old outfit — as if they never committed a crime.
        /// </summary>
        public void BreakFree(Habbo target)
        {
            // Only counts as an escape if they were actually in custody (cuffed/being escorted).
            bool wasInCustody = target != null && _states.ContainsKey(target.Id);
            RemoveCuffs(target, wipeCrimes: true);

            if (wasInCustody)
                PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(target.GetClient(), "ACH_Escaper", 1);
        }

        /// <summary>
        /// Removes a suspect's cuffs and jail data and restores their pre-cuff look. When
        /// <paramref name="wipeCrimes"/> is set their wanted record is cleared too (a clean escape);
        /// otherwise their crimes remain (e.g. being killed just knocks the cuffs off).
        /// </summary>
        private void RemoveCuffs(Habbo target, bool wipeCrimes)
        {
            if (target == null)
                return;

            _states.TryRemove(target.Id, out PoliceState state);
            RestoreLook(target, state?.OriginalLook);
            ApplyFastwalk(target, false);
            ApplyFrozen(target, false);

            PlusEnvironment.GetJailManager().Forget(target);
            if (wipeCrimes)
                PlusEnvironment.GetRpCrimeManager().ResolveAll(target.Id);
            ClearCuffPersistence(target);
        }

        /// <summary>
        /// Begins escorting a cuffed suspect to jail, carrying the star sentence captured at
        /// <c>:arrest</c>. The suspect is dragged one tile ahead of the officer (with fastwalk)
        /// until they reach a jail room.
        /// </summary>
        public void StartArrestEscort(Habbo officer, Habbo target, int pendingStars)
        {
            if (officer == null || target == null)
                return;

            PoliceState state = _states.GetOrAdd(target.Id, _ => new PoliceState { Cuffed = true, CuffedBy = officer.Id, OriginalLook = target.Look });
            state.Cuffed = true;
            state.Frozen = true;
            state.EscortedBy = officer.Id;
            state.PendingStars = pendingStars;
            ApplyFrozen(target, true);
            Persist(target, state);
        }

        public void StopEscort(int targetId)
        {
            if (_states.TryGetValue(targetId, out PoliceState state)) {
                state.EscortedBy = 0;
                GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(targetId);
                if (client?.GetHabbo() != null)
                    ApplyFastwalk(client.GetHabbo(), false);
            }
        }

        /// <summary>
        /// Handles a user disconnecting. A suspect leaving mid-escort persists a pending jail
        /// sentence (jailed on relog); a cuffed suspect otherwise persists their cuffs (restored
        /// free-to-move on relog). An officer leaving frees — but keeps cuffed — their suspects.
        /// </summary>
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

        /// <summary>
        /// Handles a user dying. If they were escorting/cuffing suspects, those suspects are freed
        /// (but stay cuffed). If the dead user was themselves cuffed, the cuffs come off.
        /// </summary>
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

                GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(targetId);
                Habbo target = targetClient?.GetHabbo();

                if (target == null) {
                    // Don't remove the state here: the client-manager lookup can fail mid-disconnect
                    // before OnDisconnect has persisted the cuffs, and removing the state first would
                    // silently release the suspect. OnDisconnect owns the cleanup.
                    continue;
                }

                // Keep frozen suspects pinned — but not while escorted, or the ClearMovement inside
                // would cancel the escort's MoveTo every tick and make them bounce in place.
                if (state.Frozen && state.EscortedBy <= 0)
                    ApplyFrozen(target, true);

                if (state.EscortedBy <= 0)
                    continue;

                GameClient officerClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(state.EscortedBy);
                Habbo officer = officerClient?.GetHabbo();

                if (officer == null || officer.CurrentRoom == null) {
                    // Officer vanished without a clean disconnect — free but keep cuffed.
                    state.EscortedBy = 0;
                    state.Frozen = false;
                    ApplyFastwalk(target, false);
                    ApplyFrozen(target, false);
                    continue;
                }

                // Follow the officer across rooms.
                if (officer.CurrentRoomId != target.CurrentRoomId) {
                    target.PrepareRoom(officer.CurrentRoomId, string.Empty);
                    continue;
                }

                // Reached a jail room — jail the suspect and end the escort.
                if (jailRooms.Contains(officer.CurrentRoomId)) {
                    if (PlusEnvironment.GetJailManager().JailFromEscort(target, officer, state.PendingStars, state.OriginalLook)) {
                        state.EscortedBy = 0;
                        state.PendingStars = 0;
                        ApplyFastwalk(target, false);
                    }
                    continue;
                }

                Room room = officer.CurrentRoom;
                RoomUser officerUser = room.GetRoomUserManager().GetRoomUserByHabbo(officer.Id);
                RoomUser targetUser = room.GetRoomUserManager().GetRoomUserByHabbo(target.Id);
                if (officerUser == null || targetUser == null)
                    continue;

                // Movement is mirrored step-for-step in OnOfficerStep (zero-delay lockstep).
                // Here we only correct drift while the officer stands still — snap the suspect
                // onto the officer's front tile and mirror their facing.
                if (officerUser.IsWalking || targetUser.Statusses.ContainsKey("mv"))
                    continue;

                Point ahead = officerUser.SquareInFront;
                if ((targetUser.X != ahead.X || targetUser.Y != ahead.Y) && room.GetGameMap().ValidTile(ahead.X, ahead.Y)) {
                    SnapTo(room, targetUser, ahead.X, ahead.Y, officerUser.RotBody);
                }
                else if (targetUser.RotBody != officerUser.RotBody || targetUser.RotHead != officerUser.RotHead) {
                    targetUser.RotBody = officerUser.RotBody;
                    targetUser.RotHead = officerUser.RotHead;
                    targetUser.UpdateNeeded = true;
                }
            }
        }

        /// <summary>
        /// Called by the room engine at the end of every user-processing cycle, after all walkers
        /// have committed their step for this tick. Any suspect being escorted by an officer who
        /// stepped this tick is given the exact same movement — same rotation, one tile ahead of
        /// the officer's next position — fusing the pair together. It must run after the user loop
        /// so the suspect's own (idle) pass can't strip the "mv" status before it's serialized.
        /// </summary>
        public void SyncEscorts(Room room)
        {
            if (room == null || _states.IsEmpty)
                return;

            foreach (var kv in _states) {
                if (kv.Value.EscortedBy <= 0)
                    continue;

                RoomUser officerUser = room.GetRoomUserManager().GetRoomUserByHabbo(kv.Value.EscortedBy);
                if (officerUser == null || !officerUser.Statusses.ContainsKey("mv"))
                    continue; // officer isn't in this room / didn't step this tick

                RoomUser suspect = room.GetRoomUserManager().GetRoomUserByHabbo(kv.Key);
                if (suspect == null)
                    continue;

                // The officer's committed step this tick: SetX/SetY, facing RotBody.
                int nextX = officerUser.SetX;
                int nextY = officerUser.SetY;
                int rot = officerUser.RotBody;

                // One tile ahead of where the officer is stepping to, in the step's direction.
                Point front = SquareInDirection(nextX, nextY, rot);
                int stepX = front.X;
                int stepY = front.Y;

                // Front tile unusable — fall back to the tile the officer is vacating.
                if (!room.GetGameMap().ValidTile(stepX, stepY)) {
                    stepX = officerUser.X;
                    stepY = officerUser.Y;
                }

                if (suspect.X == stepX && suspect.Y == stepY) {
                    suspect.RotBody = rot;
                    suspect.RotHead = rot;
                    suspect.UpdateNeeded = true;
                    continue;
                }

                double stepZ = room.GetGameMap().SqAbsoluteHeight(stepX, stepY);

                suspect.Statusses.Remove("lay");
                suspect.Statusses.Remove("sit");
                suspect.RemoveStatus("mv");
                suspect.SetStatus("mv", stepX + "," + stepY + "," + Plus.Utilities.TextHandling.GetString(stepZ));
                suspect.RotBody = rot;
                suspect.RotHead = rot;
                suspect.AllowOverride = true; // the fused step must always land, even on "blocked" tiles
                suspect.SetStep = true;
                suspect.SetX = stepX;
                suspect.SetY = stepY;
                suspect.SetZ = stepZ;
                // Pace the fused step like a real one: without this the 50ms movement tick
                // would commit it (and strip the "mv") almost immediately, cutting the walk
                // animation short.
                suspect.NextStepMs = PlusEnvironment.Now() + 500;
                suspect.UpdateNeeded = true;
            }
        }

        /// <summary>The tile one square from (x, y) in the given rotation (8 directions).</summary>
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

        /// <summary>Instantly places a suspect on a tile facing the given rotation (drift correction).</summary>
        private static void SnapTo(Room room, RoomUser roomUser, int x, int y, int rot)
        {
            roomUser.ClearMovement(true);
            room.GetGameMap().UpdateUserMovement(new Point(roomUser.X, roomUser.Y), new Point(x, y), roomUser);
            roomUser.SetPos(x, y, room.GetGameMap().SqAbsoluteHeight(x, y));
            roomUser.SetRot(rot, false);
            roomUser.UpdateNeeded = true;
        }

        /// <summary>Frees (but keeps cuffed) every suspect the given officer was escorting/holding.</summary>
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
                    ApplyFastwalk(client.GetHabbo(), false);
                    ApplyFrozen(client.GetHabbo(), false);
                    if (wasControlled)
                        client.SendWhisper(message, 1);
                }
            }
        }

        /// <summary>Rooms that count as jails: escorting a suspect into one auto-jails them.</summary>
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

        private static void RestoreLook(Habbo target, string originalLook)
        {
            if (string.IsNullOrEmpty(originalLook))
                return;

            target.Look = originalLook;
            BroadcastLook(target);
        }

        /// <summary>Replaces the shirt (.ch) part of <paramref name="originalLook"/> with the cuff figure.</summary>
        private static void ApplyCuffLook(Habbo target, string originalLook)
        {
            string cuffFigure = target.Gender != null && target.Gender.ToLower() == "f"
                ? PlusEnvironment.GetSettingsManager().TryGetValue("rp.cuff.figure.female")
                : PlusEnvironment.GetSettingsManager().TryGetValue("rp.cuff.figure.male");

            if (string.IsNullOrWhiteSpace(cuffFigure) || string.IsNullOrWhiteSpace(originalLook))
                return;

            Dictionary<string, string> lookParts = new();
            foreach (string part in originalLook.Split('.')) {
                if (string.IsNullOrWhiteSpace(part))
                    continue;
                lookParts[part.Split('-')[0]] = part;
            }

            foreach (string part in cuffFigure.Split('.')) {
                if (string.IsNullOrWhiteSpace(part))
                    continue;
                lookParts[part.Split('-')[0]] = part;
            }

            target.Look = string.Join(".", lookParts.Values);
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
        }
    }
}