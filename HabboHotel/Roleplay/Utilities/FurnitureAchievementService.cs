using System.Collections.Concurrent;
using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Roleplay.Utilities
{
    public static class FurnitureAchievementService
    {
        private const double MinuteSeconds = 60.0;

        // Maps an achievement group to the server_settings key holding its counting furniture id.
        private static readonly Dictionary<string, string> Tracked = new()
        {
            { "ACH_Jogger", "achievement.jogger.item_id" },
            { "ACH_Trampolinist", "achievement.trampolinist.item_id" },
            { "ACH_CrossTrainer", "achievement.crosstrainer.item_id" },
        };

        private sealed class StandState
        {
            public double LastSeen;      // unix seconds of the last tick the user was still standing on it (0 = not standing)
            public double AccumSeconds;  // leftover accumulated seconds not yet converted to a minute
        }

        // Key: userId + ":" + achievement group. A user is only ever in one room, so no cross-room contention.
        private static readonly ConcurrentDictionary<string, StandState> States = new();

        public static void Tick(Room room)
        {
            if (room == null)
                return;

            // Resolve the configured furniture ids once per tick (settings are in-memory, cheap).
            Dictionary<string, int> configured = null;
            foreach (KeyValuePair<string, string> entry in Tracked) {
                string raw = PlusEnvironment.GetSettingsManager().TryGetValue(entry.Value);
                if (int.TryParse(raw, out int itemId) && itemId > 0)
                    (configured ??= new Dictionary<string, int>())[entry.Key] = itemId;
            }

            if (configured == null)
                return; // nothing configured, nothing to track

            double now = PlusEnvironment.GetUnixTimestamp();

            foreach (RoomUser user in room.GetRoomUserManager().GetRoomUsers()) {
                if (user == null || user.IsBot || user.IsPet)
                    continue;

                GameClient session = user.GetClient();
                if (session?.GetHabbo() == null)
                    continue;

                List<Item> tileItems = room.GetRoomItemHandler().GetFurniObjects(user.X, user.Y);

                foreach (KeyValuePair<string, int> track in configured) {
                    string stateKey = session.GetHabbo().Id + ":" + track.Key;
                    bool standing = IsStandingOn(tileItems, track.Value);

                    if (!standing) {
                        // Stepped off — keep the leftover remainder but stop counting the gap.
                        if (States.TryGetValue(stateKey, out StandState off))
                            off.LastSeen = 0;
                        continue;
                    }

                    StandState state = States.GetOrAdd(stateKey, _ => new StandState { LastSeen = 0, AccumSeconds = 0 });
                    if (state.LastSeen > 0)
                        state.AccumSeconds += now - state.LastSeen;
                    state.LastSeen = now;

                    if (state.AccumSeconds >= MinuteSeconds) {
                        int minutes = (int)(state.AccumSeconds / MinuteSeconds);
                        state.AccumSeconds -= minutes * MinuteSeconds;
                        PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(session, track.Key, minutes);
                    }
                }
            }
        }

        private static bool IsStandingOn(List<Item> tileItems, int baseItemId)
        {
            if (tileItems == null)
                return false;

            foreach (Item item in tileItems) {
                if (item?.GetBaseItem() != null && item.GetBaseItem().Id == baseItemId)
                    return true;
            }

            return false;
        }

        public static void Forget(int userId)
        {
            foreach (string group in Tracked.Keys)
                States.TryRemove(userId + ":" + group, out _);
        }
    }
}