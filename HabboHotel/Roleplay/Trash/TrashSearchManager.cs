using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Overlay;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Roleplay.Cooldowns;
using Plus.HabboHotel.Roleplay.RpItem.Item;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Users.Roleplay;

namespace Plus.HabboHotel.Roleplay.Trash
{
    public sealed class TrashSearchManager
    {
        private readonly ConcurrentDictionary<int, ActiveTrashSearch> _activeSearches = new();
        // itemId -> userId currently diving it. Guards a single bin against two simultaneous searches.
        private readonly ConcurrentDictionary<int, int> _activeBins = new();
        private readonly ConcurrentDictionary<int, BinCooldown> _binCooldowns = new();
        private const int RewardTypeWeapon = 1;

        private readonly List<TrashReward> _rewards = [];
        private readonly Random _random = new();
        private double _nextCooldownPurgeUnix;

        public void Init()
        {
            _rewards.Clear();

            try {
                using WavePlusContext db = PlusEnvironment.GetDbContext();
                var rows = db.RpTrashRewards
                    .AsNoTracking()
                    .Where(x => x.Enabled == true)
                    .Select(x => new { x.RewardType, x.ItemId, x.ChanceWeight, x.MinAmount, x.MaxAmount })
                    .ToList();

                foreach (var row in rows) {
                    _rewards.Add(new TrashReward(
                        row.RewardType,
                        row.ItemId,
                        row.ChanceWeight,
                        row.MinAmount,
                        row.MaxAmount));
                }
            } catch {
                // Table is optional. If it does not exist, the manager falls back to all rp_items.
            }
        }

        public void Cancel(int userId)
        {
            if (!_activeSearches.TryRemove(userId, out ActiveTrashSearch search))
                return;

            _activeBins.TryRemove(search.ItemId, out _);
            CancelInteractionTimer(search.UserId);
            ResetItemState(search);
        }

        public void CancelForMovement(int userId)
        {
            if (!_activeSearches.TryRemove(userId, out ActiveTrashSearch search))
                return;

            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
            if (client != null) {
                WebOverlay.CancelInteractionTimer(client);
                client.SendWhisper("You stopped searching the bin.", 1);
            }

            // A cancelled dive leaves no trace: the user takes NO cooldown (they can immediately
            // start another dive) and the bin is reset to untouched (no bin cooldown), so the same
            // bin can be searched again right away. Only a search that actually finishes
            // (CompleteSearch) burns cooldowns and marks the bin as rummaged.
            _activeBins.TryRemove(search.ItemId, out _);
            ResetItemState(search);
        }

        public bool IsSearching(int userId) => _activeSearches.ContainsKey(userId);

        public bool StartSearch(GameClient session, Item item)
        {
            if (session?.GetHabbo() == null || item == null || item.GetRoom() == null)
                return false;

            Habbo habbo = session.GetHabbo();
            if (!IsBin(item)) {
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
            RpCooldownManager cooldowns = PlusEnvironment.GetRpCooldownManager();
            if (!cooldowns.IsReady(habbo.Id, RpCooldownKind.TrashSearch)) {
                session.SendWhisper("You can search again in " + cooldowns.RemainingSecondsCeil(habbo.Id, RpCooldownKind.TrashSearch) + " second(s).", 1);
                return false;
            }

            if (_binCooldowns.TryGetValue(item.Id, out BinCooldown binCooldown) && binCooldown.ReadyUnix > now) {
                session.SendWhisper("This bin has already been searched. Try another one.", 1);
                return false;
            }

            Room room = item.GetRoom();
            RoomUser roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (roomUser == null)
                return false;

            if (!IsInSearchRange(roomUser, item)) {
                roomUser.MoveTo(item.SquareInFront);
                session.SendWhisper("Move closer to the bin to search it.", 1);
                return false;
            }

            // Claim the bin so nobody else can dive it while this search runs. If someone already
            // holds it (and it isn't this same user), refuse.
            if (!_activeBins.TryAdd(item.Id, habbo.Id) && _activeBins.TryGetValue(item.Id, out int holder) && holder != habbo.Id) {
                session.SendWhisper("This bin is already being searched by someone else!", 1);
                return false;
            }

            int duration = RpInteractionTimer.GetSeconds(habbo);
            ActiveTrashSearch activeSearch = new()
            {
                UserId = habbo.Id,
                ItemId = item.Id,
                RoomId = room.RoomId,
                // Precise (sub-second) reference + a small grace so the reward never lands before the
                // client's visible countdown reaches zero. The whole-second `now` above is fine for
                // cooldown bookkeeping, but would complete this search up to a second early.
                FinishUnix = PlusEnvironment.GetUnixTimestampPrecise() + duration + RpInteractionTimer.CompletionGraceSeconds
            };

            if (!_activeSearches.TryAdd(habbo.Id, activeSearch)) {
                _activeBins.TryRemove(item.Id, out _);
                session.SendWhisper("You're already searching something.", 1);
                return false;
            }

            WebOverlay.SendInteractionTimer(session, "Dumpster diving finished in %seconds% seconds", duration);

            UserRpStats stats = habbo.GetRpStats();
            if (stats != null) {
                stats.Energy = Math.Max(0, stats.Energy - GetEnergyCost());
                habbo.TrySendUserStatsUpdate(true);
            }

            item.ExtraData = "1";
            item.UpdateState(false, true);

            session.GetHabbo().GetRpSkills().ProgressSkill(8, 1);
            room.SendPacket(new ShoutComposer(roomUser.VirtualId, "*searches through the bin for something valueable*", 0, 4, isRpAction: true));
            return true;
        }

        public void OnCycle()
        {
            double now = PlusEnvironment.GetUnixTimestamp();
            double preciseNow = PlusEnvironment.GetUnixTimestampPrecise();

            foreach (var shit in _activeSearches) {
                ActiveTrashSearch search = shit.Value;
                if (search.FinishUnix > preciseNow)
                    continue;

                if (_activeSearches.TryRemove(shit.Key, out ActiveTrashSearch completed))
                    CompleteSearch(completed);
            }

            if (now >= _nextCooldownPurgeUnix) {
                _nextCooldownPurgeUnix = now + 60;

                foreach (var shit in _binCooldowns) {
                    if (shit.Value.ReadyUnix <= now && _binCooldowns.TryRemove(shit.Key, out BinCooldown expired))
                        RestoreBinState(expired);
                }
            }
        }

        private void CompleteSearch(ActiveTrashSearch search)
        {
            // The bin is done being actively searched (its cooldown, applied below, is what keeps
            // others out from here) — release the live lock regardless of how the search resolves.
            _activeBins.TryRemove(search.ItemId, out _);

            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(search.UserId);
            Habbo habbo = client?.GetHabbo();
            if (habbo == null)
                return;

            Room room = habbo.CurrentRoom;
            RoomUser roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (room == null || room.RoomId != search.RoomId || roomUser == null) {
                ResetItemState(search);
                return;
            }

            ApplyBinCooldown(search);

            PlusEnvironment.GetRpCooldownManager().Apply(habbo.Id, RpCooldownKind.TrashSearch, GetUserCooldownSeconds(habbo));

            // Achievement n skill
            PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(client, "ACH_bin", 1);
            client.GetHabbo().GetRpStats().Experience += 1;
            ProgressScavenge(habbo, 1);

            int chance = GetFindChance(habbo);
            if (_random.Next(1, 101) <= chance && TryGiveReward(habbo, out string rewardText)) {
                room.SendPacket(new ShoutComposer(roomUser.VirtualId, "*finds " + rewardText + " inside of the bin*", 0, 4, isRpAction: true));
                return;
            }

            if (_random.Next(1, 101) <= chance) {
                int credits = _random.Next(1, Math.Max(2, GetMaxCreditReward() + 1));
                habbo.Credits += credits;
                client.SendPacket(new CreditBalanceComposer(habbo.Credits));
                room.SendPacket(new ShoutComposer(roomUser.VirtualId, "*finds " + credits + " credits inside of the bin*", 0, 4, isRpAction: true));
                return;
            }

            room.SendPacket(new ShoutComposer(roomUser.VirtualId, "*fails to find anything in the bin*", 0, 4, isRpAction: true));
        }

        private bool TryGiveReward(Habbo habbo, out string rewardText)
        {
            rewardText = null;

            RpItemData itemData = null;
            int amount = 1;

            TrashReward reward = RollConfiguredReward();

            if (reward != null && reward.IsWeapon)
                return TryGiveWeaponReward(habbo, reward, out rewardText);

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
            for (int i = 0; i < amount; i++) {
                if (items.AddItem(itemData.Id) != null)
                    given++;
            }

            if (given <= 0)
                return false;

            habbo.SaveRpItems();

            // Live-refresh an open inventory panel with the found item(s).
            if (habbo.GetClient() != null)
                Plus.Communication.Packets.Incoming.WebOverlayCallbackEvent.RefreshInventory(habbo.GetClient());

            rewardText = given == 1 ? $"a {itemData.Name}" : $"{given}x {itemData.Name}";
            return true;
        }

        private bool TryGiveWeaponReward(Habbo habbo, TrashReward reward, out string rewardText)
        {
            rewardText = null;

            UserRpWeapons weapons = habbo.GetRpWeapons();
            if (weapons == null || !PlusEnvironment.GetWeaponManager().TryGetWeapon(reward.ItemId, out Weapon weapon))
                return false;

            int amount = _random.Next(Math.Max(1, reward.MinAmount), Math.Max(reward.MinAmount, reward.MaxAmount) + 1);

            int given = 0;
            for (int i = 0; i < amount; i++) {
                if (weapons.AddWeapon(weapon.Id) != null)
                    given++;
            }

            if (given <= 0)
                return false;

            habbo.SaveRpWeapons();

            // Live-refresh an open inventory panel with the found weapon(s).
            if (habbo.GetClient() != null)
                Plus.Communication.Packets.Incoming.WebOverlayCallbackEvent.RefreshInventory(habbo.GetClient());

            rewardText = given == 1 ? $"a {weapon.Name}" : $"{given}x {weapon.Name}";
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
            foreach (TrashReward reward in _rewards) {
                cursor += Math.Max(0, reward.Weight);
                if (roll <= cursor)
                    return reward;
            }

            return _rewards[_rewards.Count - 1];
        }

        private static void ProgressScavenge(Habbo habbo, int amount)
        {
            try {
                habbo.ProgressSkill("Scavenge", amount);
                habbo.SaveRpSkills();
            } catch {
            }
        }

        private void ApplyBinCooldown(ActiveTrashSearch search)
        {
            if (search == null || !PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(search.RoomId, out Room room))
                return;

            Item item = room.GetRoomItemHandler().GetItem(search.ItemId);
            if (item == null)
                return;

            item.ExtraData = "1";
            item.UpdateState(false, true);
            _binCooldowns[item.Id] = new BinCooldown(room.RoomId, item.Id, PlusEnvironment.GetUnixTimestamp() + GetBinCooldownSeconds());
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

        private static void CancelInteractionTimer(int userId)
        {
            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
            if (client != null)
                WebOverlay.CancelInteractionTimer(client);
        }

        public static bool IsBin(Item item) =>
            item?.GetBaseItem()?.InteractionType == InteractionType.TrashBin;

        public static bool IsInSearchRange(RoomUser user, Item item)
        {
            if (user == null || item == null)
                return false;

            int range = Math.Max(1, GetSettingInt("rp.trash.search.range", 1));
            foreach (System.Drawing.Point point in item.GetSides()) {
                if (Math.Abs(user.X - point.X) + Math.Abs(user.Y - point.Y) <= range)
                    return true;
            }

            return Math.Abs(user.X - item.GetX) + Math.Abs(user.Y - item.GetY) <= range;
        }

        public static int GetUserCooldownSeconds(Habbo habbo)
        {
            int fallback = Math.Max(1, GetSettingInt("rp.trash.search.cooldown.seconds", 60));
            if (habbo != null && PlusEnvironment.GetGame().GetSubscriptionManager().TryGetSubscriptionData(habbo.VipRank, out Subscriptions.SubscriptionData subData) && subData.TrashSearchCooldown > 0)
                return subData.TrashSearchCooldown;

            return fallback;
        }

        public static int GetAbortedUserCooldownSeconds(Habbo habbo) => (GetUserCooldownSeconds(habbo) + 1) / 2;

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

        private readonly struct BinCooldown(int roomId, int itemId, double readyUnix)
        {
            public int RoomId { get; } = roomId;
            public int ItemId { get; } = itemId;
            public double ReadyUnix { get; } = readyUnix;
        }

        private sealed class TrashReward
        {
            public TrashReward(int rewardType, int itemId, int weight, int minAmount, int maxAmount)
            {
                RewardType = rewardType;
                ItemId = itemId;
                Weight = weight;
                MinAmount = minAmount < 1 ? 1 : minAmount;
                MaxAmount = maxAmount < MinAmount ? MinAmount : maxAmount;
            }

            public int RewardType { get; }
            public int ItemId { get; }
            public int Weight { get; }
            public int MinAmount { get; }
            public int MaxAmount { get; }

            public bool IsWeapon => RewardType == RewardTypeWeapon;
        }
    }
}