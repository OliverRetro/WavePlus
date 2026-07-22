using System.Collections.Concurrent;
using Plus.Communication.Packets.Outgoing.Roleplay;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Roleplay.TargetLock
{
    public static class TargetLockService
    {
        private sealed class TargetState
        {
            public int TargetId;
            public string TargetName;
            public bool Locked;
            public double LastUpdated;
        }

        // requesterId -> target state
        private static readonly ConcurrentDictionary<int, TargetState> Targets = new();

        public static bool IsLocked(int requesterId) =>
            Targets.TryGetValue(requesterId, out TargetState state) && state.Locked;

        public static bool HasTarget(int requesterId) =>
            Targets.TryGetValue(requesterId, out TargetState state) && state.TargetId > 0;

        /// <summary>
        /// Used by RequestWearingBadgesEvent. Return true when the UI should NOT overwrite
        /// the current clicked target because the requester already has a target locked.
        /// </summary>
        public static bool IsTargetLocked(Habbo requester, Habbo target)
        {
            if (requester == null || target == null)
                return true;

            if (requester.Id == target.Id)
                return true;

            return IsLocked(requester.Id);
        }

        public static void SetClickedUser(Habbo requester, string targetName)
        {
            if (requester == null || string.IsNullOrWhiteSpace(targetName))
                return;

            // Do not let normal user clicks replace a locked target.
            if (IsLocked(requester.Id))
                return;

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(targetName);
            Habbo target = targetClient?.GetHabbo();
            if (!IsClickable(requester, target))
                return;

            SetState(requester.Id, target.Id, target.Username, false);
        }

        /// <summary>
        /// Locks the requester's currently clicked target. Used by the web overlay.
        /// </summary>
        public static void SetLock(Habbo requester, bool locked)
        {
            if (requester == null)
                return;

            if (!locked) {
                ReleaseLock(requester, false);
                return;
            }

            Habbo target = GetTarget(requester);
            if (target == null) {
                requester.GetClient()?.SendWhisper("Click a player first, then lock your target.", 1);
                SendComposer(requester, string.Empty, false);
                return;
            }

            LockTarget(requester, target);
        }

        /// <summary>
        /// Handles a plain avatar click. A click never locks.
        /// While unlocked, freely swaps the clicked target and refreshes the RP target
        /// display (name/outfit/stats). While locked, does nothing at all — the locked
        /// target stays cached for commands and visibility.
        /// </summary>
        public static void HandleAvatarClick(Habbo requester, Habbo target)
        {
            if (requester == null || target == null)
                return;

            // Locked: ignore the click. The client optimistically shows the just-clicked
            // avatar in the RP layout, so re-assert the locked target to snap it back
            // (otherwise the clicked user lingers with empty "-" stats).
            if (IsLocked(requester.Id)) {
                Habbo locked = GetTarget(requester);
                GameClient lockedClient = requester.GetClient();
                if (locked != null && lockedClient != null) {
                    lockedClient.SendPacket(new TargetLockComposer(locked.Username, true));
                    requester.TrySendUserStatsUpdate(true);
                }
                return;
            }

            if (!IsClickable(requester, target))
                return;

            // Unlocked swap — store as a clicked (not locked) target.
            SetState(requester.Id, target.Id, target.Username, false);

            GameClient client = requester.GetClient();
            if (client == null)
                return;

            client.SendPacket(new TargetLockComposer(target.Username, false));
            requester.TrySendUserStatsUpdate(true);
        }

        public static bool LockTarget(Habbo requester, Habbo target)
        {
            if (!IsLockable(requester, target)) {
                requester?.GetClient()?.SendWhisper("That user is not a valid target.", 1);
                return false;
            }

            SetState(requester.Id, target.Id, target.Username, true);
            requester.GetClient()?.SendWhisper("Target locked: " + target.Username + ". Use :unlocktarget to release.", 1);
            SendComposer(requester, target.Username, true);
            requester.TrySendUserStatsUpdate(true);
            return true;
        }

        /// <summary>
        /// Re-asserts the lock to the client after a room change. The composer is sent even
        /// when the target is not in this room (or is offline) so the client keeps showing
        /// the lock and re-applies the outline the moment they walk back into view.
        /// </summary>
        public static void RestoreLock(Habbo requester)
        {
            if (requester == null || !Targets.TryGetValue(requester.Id, out TargetState state) || !state.Locked)
                return;

            SendComposer(requester, state.TargetName ?? string.Empty, true);
            requester.TrySendUserStatsUpdate(true);
        }

        public static void ReleaseLock(Habbo requester, bool clearTarget)
        {
            if (requester == null)
                return;

            string targetName = string.Empty;

            if (Targets.TryGetValue(requester.Id, out TargetState state)) {
                targetName = clearTarget ? string.Empty : (state.TargetName ?? string.Empty);

                if (clearTarget)
                    Targets.TryRemove(requester.Id, out _);
                else {
                    state.Locked = false;
                    state.LastUpdated = PlusEnvironment.GetUnixTimestamp();
                }
            }

            requester.GetClient()?.SendWhisper(clearTarget ? "Target released." : "Target unlocked.", 1);
            SendComposer(requester, targetName, false);
            requester.TrySendUserStatsUpdate(true);
        }

        /// <summary>
        /// Returns the currently clicked/locked online Habbo.
        ///
        /// A locked target resolves from anywhere — different room or none at all — and the
        /// lock is preserved even while the target is offline, so it snaps back when they
        /// return. An unlocked (merely clicked) target is only valid in the current room and
        /// is discarded once it goes stale.
        /// </summary>
        public static Habbo GetTarget(Habbo requester)
        {
            if (requester == null || !Targets.TryGetValue(requester.Id, out TargetState state) || state.TargetId <= 0)
                return null;

            Habbo target = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(state.TargetId)?.GetHabbo();

            if (state.Locked)
                return IsLockable(requester, target) ? target : null;

            if (!IsClickable(requester, target)) {
                Targets.TryRemove(requester.Id, out _);
                return null;
            }

            return target;
        }

        /// <summary>
        /// The locked target, or null while they are offline. The lock itself is never
        /// dropped here — only :unlocktarget, a reconnect, or locking someone else does that.
        /// </summary>
        public static Habbo GetLockedTarget(Habbo requester)
        {
            if (requester == null || !IsLocked(requester.Id))
                return null;

            return GetTarget(requester);
        }

        /// <summary>
        /// Drops a merely-clicked target (client deselected). A lock is left untouched.
        /// </summary>
        public static void ClearClickedTarget(Habbo requester)
        {
            if (requester == null || IsLocked(requester.Id))
                return;

            if (!Targets.TryRemove(requester.Id, out _))
                return;

            SendComposer(requester, string.Empty, false);
            requester.TrySendUserStatsUpdate(true);
        }

        /// <summary>
        /// Backwards-compatible name used by the overlay/stat code.
        /// </summary>
        public static Habbo GetClickedHabbo(int requesterId)
        {
            if (!Targets.TryGetValue(requesterId, out TargetState state) || state.TargetId <= 0)
                return null;

            return PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(state.TargetId)?.GetHabbo();
        }

        /// <summary>
        /// Called when this user disconnects. Only their own target is dropped — a reconnect
        /// (including a browser reload) is one of the three ways a lock is released.
        ///
        /// Locks that other people hold *on* this user are deliberately kept: they resolve to
        /// null while the target is offline and re-attach when they come back.
        /// </summary>
        public static void Clear(int requesterId)
        {
            Targets.TryRemove(requesterId, out _);

            // Anyone who merely *clicked* this user has a stale, room-scoped selection —
            // drop those, but leave real locks parked.
            foreach (var kv in Targets) {
                if (kv.Value.TargetId == requesterId && !kv.Value.Locked)
                    Targets.TryRemove(kv.Key, out _);
            }
        }

        private static void SetState(int requesterId, int targetId, string targetName, bool locked)
        {
            Targets[requesterId] = new TargetState
            {
                TargetId = targetId,
                TargetName = targetName ?? string.Empty,
                Locked = locked,
                LastUpdated = PlusEnvironment.GetUnixTimestamp()
            };
        }

        /// <summary>
        /// A lock only requires a real, distinct user. It deliberately does NOT require the
        /// two to share a room: locks survive room changes and stay parked while the target
        /// is elsewhere or offline. Commands that need the target within reach (:hit, :rob,
        /// cuffing, ...) enforce their own room/adjacency checks at the point of use.
        /// </summary>
        private static bool IsLockable(Habbo requester, Habbo target) =>
            requester != null && target != null && requester.Id != target.Id;

        /// <summary>
        /// Plain avatar clicks only ever target someone in the same room.
        /// </summary>
        private static bool IsClickable(Habbo requester, Habbo target)
        {
            if (!IsLockable(requester, target))
                return false;

            if (requester.CurrentRoom == null || target.CurrentRoom == null)
                return false;

            return requester.CurrentRoomId == target.CurrentRoomId;
        }

        private static void SendComposer(Habbo requester, string targetName, bool locked)
        {
            requester?.GetClient()?.SendPacket(new TargetLockComposer(targetName ?? string.Empty, locked));
        }
    }
}