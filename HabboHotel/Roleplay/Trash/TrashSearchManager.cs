using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Roleplay.RpItem;
using Plus.HabboHotel.Roleplay.RpItem.Item;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.PathFinding;
using Plus.HabboHotel.Users;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Plus.HabboHotel.Roleplay.Trash
{
    /// <summary>
    /// Handles RP trashbin searches. Search is time-gated, has per-user and per-bin cooldowns,
    /// awards RP items/credits, and progresses the Scavenge skill.
    /// </summary>
    public sealed class TrashSearchManager
    {
        private readonly ConcurrentDictionary<int, ActiveTrashSearch> _activeSearches = new();
        private readonly ConcurrentDictionary<int, double> _userCooldowns = new();
        private readonly ConcurrentDictionary<int, BinCooldown> _binCooldowns = new();
        private readonly List<TrashReward> _rewards = new();
        private readonly Random _random = new();
        private double _nextCooldownPurgeUnix;
        private static readonly ConcurrentDictionary<int, bool> TrashItemCache = new();

        public void Init()
        {
            _rewards.Clear();

            try
            {
                using WavePlusContext db = PlusEnvironment.GetDbContext();
                var rows = db.rp_trash_rewards
                    .AsNoTracking()
                    .Where(x => x.enabled == true)
                    .Select(x => new { x.item_id, x.chance_weight, x.min_amount, x.max_amount })
                    .ToList();

                foreach (var row in rows)
                {
                    _rewards.Add(new TrashReward(
                        row.item_id,
                        row.chance_weight,
                        row.min_amount,
                        row.max_amount));
                }
            }
            catch
            {
                // Table is optional. If it does not exist, the manager falls back to all rp_items.
            }
        }

        public void Cancel(int userId)
        {
            if (!_activeSearches.TryRemove(userId, out ActiveTrashSearch search))
                return;

            UnlockUser(search.UserId);
            ResetItemState(search);
        }

        public bool IsSearching(int userId) => _activeSearches.ContainsKey(userId);

        public bool StartSearch(GameClient session, Item item)
        {
            if (session?.GetHabbo() == null || item == null || item.GetRoom() == null)
                return false;

            Habbo habbo = session.GetHabbo();
            if (!IsTrashBin(item)) {
                session.SendWhisper("That isn't searchable trash.", 1);
                return false;
            }

            if (habbo.GetRpStats()?.IsDead == true) {
                session.SendWhisper("You can't search trash while dead.", 1);
                return false;
            }

            if (habbo.GetRpStats()?.Energy < GetEnergyCost()) {
                session.SendWhisper("You don't have enough energy to search that.", 1);
                return false;
            }

            if (_activeSearches.ContainsKey(habbo.Id)) {
                session.SendWhisper("You're already searching something.", 1);
                return false;
            }

            double now = PlusEnvironment.GetUnixTimestamp();
            if (_userCooldowns.TryGetValue(habbo.Id, out double userReady) && userReady > now) {
                session.SendWhisper("You can search again in " + (int)Math.Ceiling(userReady - now) + " second(s).", 1);
                return false;
            }

            if (_binCooldowns.TryGetValue(item.Id, out BinCooldown binCooldown) && binCooldown.ReadyUnix > now) {
                session.SendWhisper("This trashbin has already been searched. Try another one.", 1);
                return false;
            }

            Room room = item.GetRoom();
            RoomUser roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (roomUser == null)
                return false;

            if (!IsInSearchRange(roomUser, item)) {
                roomUser.MoveTo(item.SquareInFront);
                session.SendWhisper("Move closer to the trashbin to search it.", 1);
                return false;
            }

            int duration = GetDurationSeconds();
            ActiveTrashSearch activeSearch = new ActiveTrashSearch
            {
                UserId = habbo.Id,
                ItemId = item.Id,
                RoomId = room.RoomId,
                FinishUnix = now + duration
            };

            if (!_activeSearches.TryAdd(habbo.Id, activeSearch)) {
                session.SendWhisper("You're already searching something.", 1);
                return false;
            }

            roomUser.CanWalk = false;
            roomUser.ClearMovement(true);

            UserRpStats stats = habbo.GetRpStats();
            if (stats != null) {
                stats.Energy = Math.Max(0, stats.Energy - GetEnergyCost());
                habbo.TrySendUserStatsUpdate(true);
            }

            item.ExtraData = "1";
            item.UpdateState(false, true);

            session.GetHabbo().GetRpSkills().ProgressSkill(8, 1);
            room.SendPacket(new ShoutComposer(roomUser.VirtualId, "*" + habbo.Username + " searches through the bin for something valueable*", 0, 4));
            return true;
        }

        public void OnCycle()
        {
            double now = PlusEnvironment.GetUnixTimestamp();

            foreach (var kv in _activeSearches)
            {
                ActiveTrashSearch search = kv.Value;
                if (search.FinishUnix > now)
                    continue;

                if (_activeSearches.TryRemove(kv.Key, out ActiveTrashSearch completed))
                    CompleteSearch(completed);
            }

            if (now >= _nextCooldownPurgeUnix) {
                _nextCooldownPurgeUnix = now + 60;

                foreach (var kv in _userCooldowns)
                {
                    if (kv.Value <= now)
                        _userCooldowns.TryRemove(kv.Key, out _);
                }

                foreach (var kv in _binCooldowns)
                {
                    if (kv.Value.ReadyUnix <= now && _binCooldowns.TryRemove(kv.Key, out BinCooldown expired))
                        RestoreBinState(expired);
                }
            }
        }

        private void CompleteSearch(ActiveTrashSearch search)
        {
            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(search.UserId);
            Habbo habbo = client?.GetHabbo();
            if (habbo == null)
                return;

            UnlockUser(search.UserId);

            Room room = habbo.CurrentRoom;
            RoomUser roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (room == null || room.RoomId != search.RoomId || roomUser == null) {
                ResetItemState(search);
                return;
            }

            Item item = room.GetRoomItemHandler().GetItem(search.ItemId);
            if (item != null) {
                item.ExtraData = "1";
                item.UpdateState(false, true);
                _binCooldowns[item.Id] = new BinCooldown(room.RoomId, item.Id, PlusEnvironment.GetUnixTimestamp() + GetBinCooldownSeconds());
            }

            _userCooldowns[habbo.Id] = PlusEnvironment.GetUnixTimestamp() + GetUserCooldownSeconds(habbo);

            int chance = GetFindChance(habbo);
            if (_random.Next(1, 101) <= chance && TryGiveReward(habbo, out string rewardText)) {
                room.SendPacket(new ShoutComposer(roomUser.VirtualId, "*" + habbo.Username + " finds " + rewardText + " inside of the bin*", 0, 4));
                ProgressScavenge(habbo, 1);
                return;
            }

            if (_random.Next(1, 101) <= chance) {
                int credits = _random.Next(1, Math.Max(2, GetMaxCreditReward() + 1));
                habbo.Credits += credits;
                client.SendPacket(new CreditBalanceComposer(habbo.Credits));
                room.SendPacket(new ShoutComposer(roomUser.VirtualId, "*" + habbo.Username + " finds " + credits + " credits inside of the bin*", 0, 4));
                ProgressScavenge(habbo, 1);
                return;
            }

            room.SendPacket(new ShoutComposer(roomUser.VirtualId, "*" + habbo.Username + " fails to find anything in the bin*", 0, 4));
            ProgressScavenge(habbo, 1);
        }

        private bool TryGiveReward(Habbo habbo, out string rewardText)
        {
            rewardText = null;

            RpItemData itemData = null;
            int amount = 1;

            TrashReward reward = RollConfiguredReward();
            if (reward != null) {
                itemData = PlusEnvironment.GetRpItemManager().GetItemById(reward.ItemId);
                amount = _random.Next(Math.Max(1, reward.MinAmount), Math.Max(reward.MinAmount, reward.MaxAmount) + 1);
            }

            if (itemData == null) {
                List<RpItemData> allItems = PlusEnvironment.GetRpItemManager().GetItems().ToList();
                if (allItems.Count == 0)
                    return false;

                itemData = allItems[_random.Next(0, allItems.Count)];
                amount = 1;
            }

            if (itemData == null)
                return false;

            UserRpItems items = habbo.GetRpItems();
            if (items == null)
                return false;

            int given = 0;
            for (int i = 0; i < amount; i++)
            {
                if (items.AddItem(itemData.Id) != null)
                    given++;
            }

            if (given <= 0)
                return false;

            habbo.SaveRpItems();
            rewardText = given == 1 ? $"a {itemData.Name}" : $"{given}x {itemData.Name}";
            return true;
        }

        private TrashReward RollConfiguredReward()
        {
            if (_rewards.Count == 0)
                return null;

            int totalWeight = _rewards.Sum(x => Math.Max(0, x.Weight));
            if (totalWeight <= 0)
                return null;

            int roll = _random.Next(1, totalWeight + 1);
            int cursor = 0;
            foreach (TrashReward reward in _rewards)
            {
                cursor += Math.Max(0, reward.Weight);
                if (roll <= cursor)
                    return reward;
            }

            return _rewards[_rewards.Count - 1];
        }

        private static void ProgressScavenge(Habbo habbo, int amount)
        {
            try
            {
                habbo.ProgressSkill("Scavenge", amount);
                habbo.SaveRpSkills();
            }
            catch
            {
            }
        }

        private static void ResetItemState(ActiveTrashSearch search)
        {
            if (search == null)
                return;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(search.RoomId, out Room room))
                return;

            Item item = room.GetRoomItemHandler().GetItem(search.ItemId);
            if (item == null)
                return;

            item.ExtraData = "0";
            item.UpdateState(false, true);
        }

        private static void RestoreBinState(BinCooldown cooldown)
        {
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(cooldown.RoomId, out Room room))
                return;

            Item item = room.GetRoomItemHandler().GetItem(cooldown.ItemId);
            if (item == null)
                return;

            item.ExtraData = "0";
            item.UpdateState(false, true);
        }

        private static void UnlockUser(int userId)
        {
            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
            RoomUser roomUser = client?.GetHabbo()?.CurrentRoom?.GetRoomUserManager().GetRoomUserByHabbo(userId);
            if (roomUser == null)
                return;

            // Do not override police cuffs.
            if (PlusEnvironment.GetPoliceManager()?.IsCuffed(userId) == true)
                return;

            roomUser.CanWalk = true;
            roomUser.UpdateNeeded = true;
        }

        public static bool IsTrashBin(Item item)
        {
            ItemData baseItem = item?.GetBaseItem();
            if (baseItem == null)
                return false;

            return TrashItemCache.GetOrAdd(baseItem.Id, _ =>
            {
                if (baseItem.InteractionType == InteractionType.TrashBin)
                    return true;

                string itemName = baseItem.ItemName ?? string.Empty;
                string publicName = baseItem.PublicName ?? string.Empty;
                string combined = (itemName + " " + publicName).ToLowerInvariant();

                return combined.Contains("trash") ||
                       combined.Contains("garbage") ||
                       combined.Contains("dumpster") ||
                       combined.Contains("rubbish") ||
                       combined.Split(new[] { ' ', '_', '-', '.', '/' }, StringSplitOptions.RemoveEmptyEntries).Contains("bin");
            });
        }

        public static bool IsInSearchRange(RoomUser user, Item item)
        {
            if (user == null || item == null)
                return false;

            int range = Math.Max(1, GetSettingInt("rp.trash.search.range", 1));
            foreach (System.Drawing.Point point in item.GetSides())
            {
                if (Math.Abs(user.X - point.X) + Math.Abs(user.Y - point.Y) <= range)
                    return true;
            }

            return Math.Abs(user.X - item.GetX) + Math.Abs(user.Y - item.GetY) <= range;
        }

        public static int GetDurationSeconds() => Math.Max(1, GetSettingInt("rp.trash.search.duration.seconds", 5));

        public static int GetUserCooldownSeconds(Habbo habbo)
        {
            int fallback = Math.Max(1, GetSettingInt("rp.trash.search.cooldown.seconds", 60));
            if (habbo != null && PlusEnvironment.GetGame().GetSubscriptionManager().TryGetSubscriptionData(habbo.VipRank, out Subscriptions.SubscriptionData subData) && subData.TrashSearchCooldown > 0)
                return subData.TrashSearchCooldown;

            return fallback;
        }

        public static int GetBinCooldownSeconds() => Math.Max(1, GetSettingInt("rp.trash.bin.cooldown.seconds", 120));
        public static int GetEnergyCost() => Math.Max(0, GetSettingInt("rp.trash.search.energy.cost", 2));
        public static int GetMaxCreditReward() => Math.Max(1, GetSettingInt("rp.trash.search.max.credits", 8));

        private static int GetFindChance(Habbo habbo)
        {
            int chance = Math.Clamp(GetSettingInt("rp.trash.search.item.chance", 35), 1, 100);
            int scavengeLevel = habbo?.GetRpSkills()?.GetLevelByProgressCategory("Scavenge") ?? 0;
            int scavengerBoost = UserRpStats.GetScavengerBoost(habbo);
            return Math.Clamp(chance + (scavengeLevel * 5) + scavengerBoost, 1, 95);
        }

        private static int GetSettingInt(string key, int fallback)
        {
            string value = PlusEnvironment.GetSettingsManager()?.TryGetValue(key);
            return int.TryParse(value, out int parsed) ? parsed : fallback;
        }

        private sealed class ActiveTrashSearch
        {
            public int UserId;
            public int ItemId;
            public int RoomId;
            public double FinishUnix;
        }

        private readonly struct BinCooldown
        {
            public BinCooldown(int roomId, int itemId, double readyUnix)
            {
                RoomId = roomId;
                ItemId = itemId;
                ReadyUnix = readyUnix;
            }

            public int RoomId { get; }
            public int ItemId { get; }
            public double ReadyUnix { get; }
        }

        private sealed class TrashReward
        {
            public TrashReward(int itemId, int weight, int minAmount, int maxAmount)
            {
                ItemId = itemId;
                Weight = weight;
                MinAmount = minAmount < 1 ? 1 : minAmount;
                MaxAmount = maxAmount < MinAmount ? MinAmount : maxAmount;
            }

            public int ItemId { get; }
            public int Weight { get; }
            public int MinAmount { get; }
            public int MaxAmount { get; }
        }
    }
}
