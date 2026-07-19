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

            if (requester.CurrentRoom == null || target.CurrentRoom == null || requester.CurrentRoomId != target.CurrentRoomId)
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
            if (!IsValidTarget(requester, target))
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

            if (!locked)
            {
                ReleaseLock(requester, false);
                return;
            }

            Habbo target = GetTarget(requester);
            if (target == null)
            {
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
            if (IsLocked(requester.Id))
            {
                Habbo locked = GetTarget(requester);
                GameClient lockedClient = requester.GetClient();
                if (locked != null && lockedClient != null)
                {
                    lockedClient.SendPacket(new TargetLockComposer(locked.Username, true));
                    requester.TrySendUserStatsUpdate(true);
                }
                return;
            }

            if (!IsValidTarget(requester, target))
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
            if (!IsValidTarget(requester, target))
            {
                requester?.GetClient()?.SendWhisper("That user is not a valid target.", 1);
                return false;
            }

            SetState(requester.Id, target.Id, target.Username, true);
            requester.GetClient()?.SendWhisper("Target locked: " + target.Username + ". Use :unlocktarget to release.", 1);
            SendComposer(requester, target.Username, true);
            requester.TrySendUserStatsUpdate(true);
            return true;
        }

        public static void RestoreLock(Habbo requester)
        {
            if (requester == null || !IsLocked(requester.Id))
                return;

            // Only meaningful when the locked target is in the room we just entered otherwise the lock stays parked until they meet again.
            Habbo target = GetTarget(requester);

            if (target == null)
                return;

            SendComposer(requester, target.Username, true);
            requester.TrySendUserStatsUpdate(true);
        }

        public static void ReleaseLock(Habbo requester, bool clearTarget)
        {
            if (requester == null)
                return;

            string targetName = string.Empty;

            if (Targets.TryGetValue(requester.Id, out TargetState state))
            {
                targetName = clearTarget ? string.Empty : (state.TargetName ?? string.Empty);

                if (clearTarget)
                    Targets.TryRemove(requester.Id, out _);
                else
                {
                    state.Locked = false;
                    state.LastUpdated = PlusEnvironment.GetUnixTimestamp();
                }
            }

            requester.GetClient()?.SendWhisper(clearTarget ? "Target released." : "Target unlocked.", 1);
            SendComposer(requester, targetName, false);
            requester.TrySendUserStatsUpdate(true);
        }

        /// <summary>
        /// Returns the currently clicked/locked online Habbo, or null if stale/offline/outside the room.
        /// </summary>
        public static Habbo GetTarget(Habbo requester)
        {
            if (requester == null || !Targets.TryGetValue(requester.Id, out TargetState state) || state.TargetId <= 0)
                return null;

            Habbo target = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(state.TargetId)?.GetHabbo();
            if (!IsValidTarget(requester, target))
            {
                if (!state.Locked)
                    Targets.TryRemove(requester.Id, out _);
                return null;
            }

            return target;
        }

        public static Habbo GetLockedTarget(Habbo requester)
        {
            if (requester == null || !IsLocked(requester.Id))
                return null;

            Habbo target = GetTarget(requester);
            if (target == null)
            {
                ReleaseLock(requester, true);
                requester.GetClient()?.SendWhisper("Your locked target is no longer available.", 1);
            }

            return target;
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

        public static void Clear(int requesterId)
        {
            Targets.TryRemove(requesterId, out _);

            // Also release anyone who had this user locked/clicked.
            foreach (var kv in Targets)
            {
                if (kv.Value.TargetId == requesterId)
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

        private static bool IsValidTarget(Habbo requester, Habbo target)
        {
            if (requester == null || target == null)
                return false;

            if (requester.Id == target.Id)
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
