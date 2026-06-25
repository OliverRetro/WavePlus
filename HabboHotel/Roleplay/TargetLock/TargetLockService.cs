using System;
using System.Collections.Concurrent;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Roleplay.TargetLock
{
    public static class TargetLockService
    {
        // requesterId -> clicked target username
        private static readonly ConcurrentDictionary<int, string> ClickedTargets = new();
        // requesterId -> lock flag
        private static readonly ConcurrentDictionary<int, bool> Locked = new();

        public static bool IsLocked(int requesterId) =>
            Locked.TryGetValue(requesterId, out bool locked) && locked;

        public static void SetLock(Habbo requester, bool locked)
        {
            if (requester == null)
                return;

            if (locked)
                Locked[requester.Id] = true;
            else
                Locked.TryRemove(requester.Id, out _);

            requester.GetClient()?.SendWhisper(locked
                ? "You have locked on to your target"
                : "You have unlocked your target");
        }
        public static bool IsTargetLocked(Habbo requester, Habbo target)
        {
            if (requester == null || target == null)
                return true;

            if (target.Id == requester.Id)
                return true;

            if (requester.CurrentRoom == null || target.CurrentRoom == null
                || requester.CurrentRoom != target.CurrentRoom)
                return true;

            return IsLocked(requester.Id);
        }

        public static void SetClickedUser(Habbo requester, string targetName)
        {
            if (requester == null || string.IsNullOrEmpty(targetName))
                return;

            ClickedTargets[requester.Id] = targetName;
        }

        /// <summary>
        /// Returns the online Habbo a requester currently has clicked/locked, or null.
        /// </summary>
        public static Habbo GetClickedHabbo(int requesterId)
        {
            if (!ClickedTargets.TryGetValue(requesterId, out string name) || string.IsNullOrEmpty(name))
                return null;

            return PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(name)?.GetHabbo();
        }

        public static void Clear(int requesterId)
        {
            ClickedTargets.TryRemove(requesterId, out _);
            Locked.TryRemove(requesterId, out _);
        }
    }
}
