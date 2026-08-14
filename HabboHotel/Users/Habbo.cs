using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Incoming.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Quests;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Core;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items.Interactor;
using Plus.HabboHotel.Roleplay.Banking;
using Plus.HabboHotel.Roleplay.RpItem.Item;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Roleplay.Skill;
using Plus.HabboHotel.Roleplay.Stock;
using Plus.HabboHotel.Roleplay.TargetLock;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboHotel.Subscriptions;
using Plus.HabboHotel.Users.Badges;
using Plus.HabboHotel.Users.Clothing;
using Plus.HabboHotel.Users.Effects;
using Plus.HabboHotel.Users.Ignores;
using Plus.HabboHotel.Users.Inventory;
using Plus.HabboHotel.Users.Messenger;
using Plus.HabboHotel.Users.Messenger.FriendBar;
using Plus.HabboHotel.Users.Navigator.SavedSearches;
using Plus.HabboHotel.Users.Permissions;
using Plus.HabboHotel.Users.Process;
using Plus.HabboHotel.Users.Relationships;
using Plus.HabboHotel.Users.Roleplay;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
// Disambiguates the bare names from the scaffolded EF entities Plus.Database.EF.Entities.Room/UserAchievement.
using Room = Plus.HabboHotel.Rooms.Room;
using UserAchievement = Plus.HabboHotel.Achievements.UserAchievement;
using UserRpItem = Plus.HabboHotel.Roleplay.RpItem.Item.UserRpItem;

namespace Plus.HabboHotel.Users
{
    public class Habbo
    {
        public sealed class HomeRoomDataState
        {
            public int roomid { get; set; }
            public int x { get; set; }
            public int y { get; set; }
            public double z { get; set; }
            public int rotation { get; set; }
            public int weapon { get; set; }
        }

        // Generic player values.
        public int Id { get; set; }
        public string Username { get; set; }
        public int Rank { get; set; }
        public string Motto { get; set; }
        public string OldMotto { get; set; }
        public string Look { get; set; }
        public string OldLook { get; set; }
        // Cached corporation/business group the user works for (0 = none). A user can only have one.
        public int CorporationId { get; set; }
        public string Gender { get; set; }
        public string FootballLook { get; set; }
        public string FootballGender { get; set; }
        public int Credits { get; set; }
        public int Duckets { get; set; }
        public int Diamonds { get; set; }
        public int GotwPoints { get; set; }
        public int HomeRoom { get; set; }
        public double LastOnline { get; set; }
        public double AccountCreated { get; set; }
        public List<int> ClientVolume { get; set; }
        public double LastNameChange { get; set; }
        public string MachineId { get; set; }
        public bool ChatPreference { get; set; }
        public bool FocusPreference { get; set; }
        public bool IsExpert { get; set; }
        public int VipRank { get; set; }

        // Unix timestamp (seconds) at which the user's timed VIP subscription lapses.
        // 0 = no active subscription. VipRank is kept as the 0/1 "has VIP" flag.
        public int VipExpire { get; set; }

        public bool IsVip => VipExpire > (int)PlusEnvironment.GetUnixTimestamp();

        // Whole minutes of VIP time left (clamped at 0). Drives the purse countdown.
        public int VipMinutesRemaining
        {
            get
            {
                int remaining = VipExpire - (int)PlusEnvironment.GetUnixTimestamp();
                return remaining <= 0 ? 0 : (remaining + 59) / 60;
            }
        }

        // Abilities triggered by generic events.
        public bool AppearOffline { get; set; }
        public bool AllowTradingRequests { get; set; }
        public bool AllowUserFollowing { get; set; }
        public bool AllowFriendRequests { get; set; }
        public bool AllowMessengerInvites { get; set; }
        public bool AllowPetSpeech { get; set; }
        public bool AllowBotSpeech { get; set; }
        public bool AllowPublicRoomStatus { get; set; }
        public bool AllowConsoleMessages { get; set; }
        public bool ClickThrough { get; set; }
        public bool AllowGifts { get; set; }
        public bool AllowMimic { get; set; }
        public bool ReceiveWhispers { get; set; }
        public bool IgnorePublicWhispers { get; set; }
        public bool PlayingFastFood { get; set; }
        public FriendBarState FriendBarState { get; set; }
        public int ChristmasDay { get; set; }
        public int WantsToRideHorse { get; set; }
        public int TimeAfk { get; set; }
        public bool DisableForcedEffects { get; set; }

        // Player saving.
        private bool _disconnected;
        private bool _habboSaved;
        public bool ChangingName { get; set; }

        // Counters
        public int FriendCount { get; set; }
        public double FloodTime { get; set; }
        public double TimeMuted { get; set; }
        public double TradingLockExpiry { get; set; }
        public int BannedPhraseCount { get; set; }
        public double SessionStart { get; set; }
        public int MessengerSpamCount { get; set; }
        public double MessengerSpamTime { get; set; }
        public int CreditsUpdateTick { get; set; }
        public int BankSaveTick { get; set; }

        // Room related
        public int TentId { get; set; }
        public int HopperId { get; set; }
        public bool IsHopping { get; set; }
        public int TeleportId { get; set; }
        public bool IsTeleporting { get; set; }
        public int TeleportingRoomId { get; set; }
        public bool RoomAuthOk { get; set; }
        public int CurrentRoomId { get; set; }

        // Walking mode set by :fastwalk / :superfastwalk. RoomUser is rebuilt on every
        // room entry, so the chosen mode is held here and re-applied on arrival.
        // Temporary fastwalk (RP items, police escorts) deliberately stays room-local.
        public bool FastWalkingEnabled { get; set; }
        public bool SuperFastWalkingEnabled { get; set; }

        // Advertising reporting system.
        public bool HasSpoken { get; set; }
        public double LastAdvertiseReport { get; set; }
        public bool AdvertisingReported { get; set; }
        public bool AdvertisingReportedBlocked { get; set; }

        // Values generated within the game.
        public bool WiredInteraction { get; set; }
        public int QuestLastCompleted { get; set; }
        public bool InventoryAlert { get; set; }
        public bool IgnoreBobbaFilter { get; set; }
        public bool WiredTeleporting { get; set; }
        public int CustomBubbleId { get; set; }
        public List<int> OwnedChatBubbleIds { get; set; }
        public int TempInt { get; set; }
        public double LastWorkActionTime { get; set; }
        public double LastUserStatsUpdate { get; set; }
        public bool OnHelperDuty { get; set; }
        public HomeRoomDataState HomeRoomData { get; set; }
        public bool PendingReconnectPlacement { get; set; }
        public int CachedSkillHealth { get; set; }
        public bool HasCachedSkillHealth { get; set; }

        // FastFood
        public int FastFoodScore { get; set; }

        // Just random fun stuff.
        public int PetId { get; set; }

        // Anti-script placeholders.
        public DateTime LastGiftPurchaseTime { get; set; }
        public DateTime LastMottoUpdateTime { get; set; }
        public DateTime LastClothingUpdateTime { get; set; }
        public DateTime LastForumMessageUpdateTime { get; set; }

        public int GiftPurchasingWarnings { get; set; }
        public int MottoUpdateWarnings { get; set; }
        public int ClothingUpdateWarnings { get; set; }

        public bool SessionGiftBlocked { get; set; }
        public bool SessionMottoBlocked { get; set; }
        public bool SessionClothingBlocked { get; set; }

        public List<int> RatedRooms;

        private GameClient _client;
        private readonly HabboStats _habboStats;
        private HabboMessenger _messenger;
        private ProcessComponent _process;
        public ArrayList FavoriteRooms;
        public Dictionary<int, int> Quests;
        private BadgeComponent _badgeComponent;
        private InventoryComponent _inventoryComponent;
        public Dictionary<int, Relationship> Relationships;
        public ConcurrentDictionary<string, UserAchievement> Achievements;

        private readonly DateTime _timeCached;

        private SearchesComponent _navigatorSearches;
        private EffectsComponent _fx;
        private ClothingComponent _clothing;
        private PermissionComponent _permissions;
        private IgnoresComponent _ignores;

        private UserRpStats _rpStats;
        private UserRpSkills _rpSkills;
        private UserRpWeapons _rpWeapons;
        private UserRpWeaponSkins _rpWeaponSkins;
        private UserRpItems _rpItems;
        private UserCraftingRecipes _craftingRecipes;
        private UserRpBankAccount _bankAccount;
        private Plus.HabboHotel.Roleplay.Settings.UserRpSettings _rpSettings;
        private CancellationTokenSource _aggressionTimerTokenSource;
        private DateTime? _aggressionExpiresAtUtc;

        public double CustomBuildHeight { get; set; }

        public Habbo(int id, string username, int rank, string motto, string look, string gender, int credits, int activityPoints, int homeRoom,
            bool hasFriendRequestsDisabled, int lastOnline, bool appearOffline, bool hideInRoom, double createDate, int diamonds,
            string machineId, string clientVolume, bool chatPreference, bool focusPreference, bool petsMuted, bool botsMuted, bool advertisingReportBlocked, double lastNameChange,
            int gotwPoints, bool ignoreInvites, double timeMuted, double tradingLock, bool allowGifts, int friendBarState, bool disableForcedEffects, bool allowMimic, int vipRank, int vipExpire, int bubble)
        {
            Id = id;
            Username = username;
            Rank = rank;
            Motto = motto;
            Look = look;
            Gender = gender.ToLower();
            FootballLook = PlusEnvironment.FilterFigure(look.ToLower());
            FootballGender = gender.ToLower();
            Credits = credits;
            Duckets = activityPoints;
            Diamonds = diamonds;
            GotwPoints = gotwPoints;
            HomeRoom = homeRoom;
            LastOnline = lastOnline;
            AccountCreated = createDate;
            ClientVolume = new List<int>();
            foreach (string str in clientVolume.Split(',')) {
                if (int.TryParse(str, out _))
                    ClientVolume.Add(int.Parse(str));
                else
                    ClientVolume.Add(100);
            }

            LastNameChange = lastNameChange;
            MachineId = machineId;
            ChatPreference = chatPreference;
            FocusPreference = focusPreference;
            IsExpert = IsExpert;

            AppearOffline = appearOffline;
            AllowTradingRequests = true; //TODO
            AllowUserFollowing = true; //TODO
            AllowFriendRequests = hasFriendRequestsDisabled; //TODO
            AllowMessengerInvites = ignoreInvites;
            AllowPetSpeech = petsMuted;
            AllowBotSpeech = botsMuted;
            AllowPublicRoomStatus = hideInRoom;
            AllowConsoleMessages = true;
            AllowGifts = allowGifts;
            AllowMimic = allowMimic;
            ReceiveWhispers = true;
            IgnorePublicWhispers = false;
            PlayingFastFood = false;
            FriendBarState = FriendBarStateUtility.GetEnum(friendBarState);
            ChristmasDay = ChristmasDay;
            WantsToRideHorse = 0;
            TimeAfk = 0;
            DisableForcedEffects = disableForcedEffects;
            VipRank = vipRank;
            VipExpire = vipExpire;

            _disconnected = false;
            _habboSaved = false;
            ChangingName = false;

            FloodTime = 0;
            FriendCount = 0;
            TimeMuted = timeMuted;
            _timeCached = DateTime.Now;

            TradingLockExpiry = tradingLock;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                if (!db.UserInfos.Any(u => u.UserId == id)) {
                    db.UserInfos.Add(new UserInfoEntity { UserId = id });
                    db.SaveChanges();
                }

                if (TradingLockExpiry > 0 && PlusEnvironment.GetUnixTimestamp() > TradingLockExpiry) {
                    TradingLockExpiry = 0;
                    db.UserInfos.Where(u => u.UserId == id)
                        .ExecuteUpdate(s => s.SetProperty(u => u.TradingLocked, 0.0));
                }
            }

            StartAggressionTimer(_rpStats?.Aggression ?? 0);

            OldMotto = "Citizen";
            OldLook = "Citizen";

            BannedPhraseCount = 0;
            SessionStart = PlusEnvironment.GetUnixTimestamp();
            MessengerSpamCount = 0;
            MessengerSpamTime = 0;
            CreditsUpdateTick = Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("user.currency_scheduler.tick"));
            BankSaveTick = 900;

            TentId = 0;
            HopperId = 0;
            IsHopping = false;
            TeleportId = 0;
            IsTeleporting = false;
            TeleportingRoomId = 0;
            RoomAuthOk = false;
            CurrentRoomId = 0;

            HasSpoken = false;
            LastAdvertiseReport = 0;
            AdvertisingReported = false;
            AdvertisingReportedBlocked = advertisingReportBlocked;

            WiredInteraction = false;
            QuestLastCompleted = 0;
            InventoryAlert = false;
            IgnoreBobbaFilter = false;
            WiredTeleporting = false;
            CustomBubbleId = bubble;
            OwnedChatBubbleIds = new List<int>();
            OnHelperDuty = false;
            FastFoodScore = 0;
            PetId = 0;
            TempInt = 0;

            LastGiftPurchaseTime = DateTime.Now;
            LastMottoUpdateTime = DateTime.Now;
            LastClothingUpdateTime = DateTime.Now;
            LastForumMessageUpdateTime = DateTime.Now;

            GiftPurchasingWarnings = 0;
            MottoUpdateWarnings = 0;
            ClothingUpdateWarnings = 0;

            SessionGiftBlocked = false;
            SessionMottoBlocked = false;
            SessionClothingBlocked = false;

            FavoriteRooms = new ArrayList();
            Achievements = new ConcurrentDictionary<string, UserAchievement>();
            Relationships = new Dictionary<int, Relationship>();
            RatedRooms = new List<int>();

            CustomBuildHeight = -1;

            //TODO: Nope.
            InitPermissions();

            #region Stats

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                UserStatEntity statRow = db.UserStats.FirstOrDefault(s => s.Id == id);
                if (statRow == null) {
                    statRow = new UserStatEntity { Id = id };
                    db.UserStats.Add(statRow);
                    db.SaveChanges();
                }

                try {
                    _habboStats = new HabboStats(
                        statRow.RoomVisits,
                        statRow.OnlineTime,
                        statRow.Respect,
                        statRow.RespectGiven,
                        statRow.GiftsGiven,
                        statRow.GiftsReceived,
                        statRow.DailyRespectPoints,
                        statRow.DailyPetRespectPoints,
                        statRow.AchievementScore,
                        (int)statRow.QuestId,
                        statRow.QuestProgress,
                        statRow.Groupid,
                        statRow.RespectsTimestamp,
                        statRow.ForumPosts
                    );

                    if (statRow.RespectsTimestamp != DateTime.Today.ToString("MM/dd")) {
                        _habboStats.RespectsTimestamp = DateTime.Today.ToString("MM/dd");

                        int dailyRespects = 10;

                        if (_permissions.HasRight("mod_tool"))
                            dailyRespects = 20;
                        else if (PlusEnvironment.GetGame().GetSubscriptionManager().TryGetSubscriptionData(vipRank, out SubscriptionData subData))
                            dailyRespects = subData.Respects;

                        _habboStats.DailyRespectPoints = dailyRespects;
                        _habboStats.DailyPetRespectPoints = dailyRespects;

                        string respectsTimestamp = DateTime.Today.ToString("MM/dd");
                        db.UserStats.Where(s => s.Id == id)
                            .ExecuteUpdate(s => s
                                .SetProperty(x => x.DailyRespectPoints, dailyRespects)
                                .SetProperty(x => x.DailyPetRespectPoints, dailyRespects)
                                .SetProperty(x => x.RespectsTimestamp, respectsTimestamp));
                    }
                } catch (Exception e) {
                    ExceptionLogger.LogException(e);
                }

                List<int> chatBubbleIds = db.UserChatBubbles.Where(b => b.UserId == id).Select(b => b.ChatBubbleId).ToList();
                foreach (int chatBubbleId in chatBubbleIds) {
                    if (!OwnedChatBubbleIds.Contains(chatBubbleId))
                        OwnedChatBubbleIds.Add(chatBubbleId);
                }

                OwnedChatBubbleIds.Add(0);
            }

            _bankAccount = PlusEnvironment.GetBankingManager().LoadAccount(id);

            _rpSettings = Plus.HabboHotel.Roleplay.Settings.UserRpSettings.Load(id);
            // Keep the legacy runtime flag in sync with the persisted setting.
            ClickThrough = _rpSettings.ClickThrough;

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(_habboStats.FavouriteGroupId, out Groups.Group _))
                _habboStats.FavouriteGroupId = 0;

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                UserRpStatisticEntity statRow = db.UserRpStatistics.FirstOrDefault(s => s.UserId == id);
                if (statRow == null) {
                    statRow = new UserRpStatisticEntity { UserId = id };
                    db.UserRpStatistics.Add(statRow);
                    db.SaveChanges();
                }

                try {
                    _rpStats = new UserRpStats(
                        id,
                        statRow.Health,
                        statRow.Shield,
                        statRow.Energy,
                        statRow.Hunger,
                        statRow.Experience,
                        statRow.Knockouts,
                        statRow.Deaths,
                        statRow.Arrested,
                        statRow.Escapes,
                        (int)statRow.DamageDealt,
                        (int)statRow.DamageTaken,
                        statRow.Attacks,
                        statRow.Attacked,
                        statRow.Knowledge,
                        statRow.Strength,
                        statRow.IsDead == 1,
                        statRow.HospitalReleaseTime,
                        statRow.HospitalHealStartTime,
                        statRow.HospitalHealthStart,
                        statRow.Aggression,
                        statRow.PassiveMode == 1,
                        statRow.Robberies,
                        statRow.Robbed,
                        statRow.IsCuffed,
                        statRow.JailReleaseTime,
                        statRow.JailPending,
                        statRow.JailStars,
                        statRow.JailRevertLook ?? "",
                        statRow.JailRoomId,
                        statRow.JailTimeLeft
                    );
                    _rpStats.AttributePoints = statRow.AttributePoints;
                } catch (Exception e) {
                    ExceptionLogger.LogException(e);
                }
            }

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var weaponRows = db.UserRpWeapons.Where(w => w.UserId == id)
                    .Select(w => new { w.Id, w.WeaponId, w.DurabilityLeft, w.Slot }).ToList();

                List<UserWeapon> loadedWeapons = new();
                foreach (var row in weaponRows) {
                    Weapon weaponData = PlusEnvironment.GetWeaponManager().GetWeaponById(row.WeaponId);
                    if (weaponData == null)
                        continue;

                    loadedWeapons.Add(new UserWeapon(row.Id, id, row.WeaponId, row.DurabilityLeft, weaponData, false, row.Slot));
                }

                _rpWeapons = new UserRpWeapons(id, PlusEnvironment.GetWeaponManager(), loadedWeapons);
                _rpWeapons.MarkSaved();
                _rpStats.Weapons = _rpWeapons;
            }

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var skinRows = db.UserRpWeaponSkins.Where(s => s.UserId == id)
                    .Select(s => new { s.Id, s.SkinId, s.Equipped }).ToList();

                List<UserWeaponSkin> loadedSkins = new();
                foreach (var row in skinRows) {
                    WeaponSkin skinData = PlusEnvironment.GetWeaponManager().GetSkinById(row.SkinId);
                    if (skinData == null)
                        continue;

                    loadedSkins.Add(new UserWeaponSkin(row.Id, id, skinData.Id, row.Equipped == 1, skinData));
                }

                _rpWeaponSkins = new UserRpWeaponSkins(loadedSkins);
                _rpWeaponSkins.MarkSaved();
            }

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var skillRows = db.UserRpSkills.Where(s => s.UserId == id)
                    .Select(s => new { s.SkillId, s.Progress, s.Equipped }).ToList();

                List<UserSkillData> loadedSkills = new();
                foreach (var row in skillRows) {
                    loadedSkills.Add(new UserSkillData(row.SkillId, row.Progress, row.Equipped));
                }

                _rpSkills = new UserRpSkills(PlusEnvironment.GetSkillManager(), loadedSkills);
                _rpSkills.MarkSaved();
            }

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var itemRows = db.UserRpItems.Where(i => i.UserId == id)
                    .Select(i => new { i.Id, i.ItemId, i.Uses, i.Slot, i.DurabilityLeft, i.Equipped }).ToList();

                List<UserRpItem> loadedItems = new();
                foreach (var row in itemRows) {
                    RpItemData itemData = PlusEnvironment.GetRpItemManager().GetItemById(row.ItemId);
                    if (itemData == null)
                        continue;

                    loadedItems.Add(new UserRpItem(row.Id, id, row.ItemId, row.Uses, itemData, row.Slot, row.DurabilityLeft, row.Equipped));
                }

                _rpItems = new UserRpItems(id, PlusEnvironment.GetRpItemManager(), loadedItems);
                _rpItems.MarkSaved();

                // Restore the equipped shield's durability into the live shield stat.
                UserRpItem equippedShield = _rpItems.GetEquippedShield();
                if (equippedShield != null && _rpStats != null)
                    _rpStats.Shield = equippedShield.EffectiveDurability;
            }

            try {
                using WavePlusContext db = PlusEnvironment.GetDbContext();
                var recipeRows = db.UserCraftingRecipes.Where(r => r.UserId == id)
                    .Select(r => new { r.RecipeId, r.Revealed }).ToList();

                _craftingRecipes = new UserCraftingRecipes(id,
                    recipeRows.Select(r => (r.RecipeId, r.Revealed)));
            } catch {
                // Table not present yet (Crafting.sql not applied) — never block login over it.
                _craftingRecipes = new UserCraftingRecipes(id, System.Array.Empty<(int, bool)>());
            }

            #endregion
        }

        public IChatCommand ChatCommand { get; set; }

        public HabboStats GetStats()
        {
            return _habboStats;
        }

        public UserRpStats GetRpStats()
        {
            SyncAggressionState();
            return _rpStats;
        }

        public UserRpBankAccount GetBankAccount()
        {
            return _bankAccount;
        }

        public Plus.HabboHotel.Roleplay.Settings.UserRpSettings GetRpSettings()
        {
            return _rpSettings;
        }

        public int GetAggressionSecondsRemaining()
        {
            SyncAggressionState();
            return _rpStats?.Aggression ?? 0;
        }

        public int GetAggressionDuration()
        {
            int aggressionDuration = 30;
            if (PlusEnvironment.GetGame().GetSubscriptionManager().TryGetSubscriptionData(VipRank, out SubscriptionData subData))
                aggressionDuration = Math.Max(0, aggressionDuration - subData.StaticAggression);

            return aggressionDuration;
        }

        public void ResetAggressionTimer()
        {
            StartAggressionTimer(GetAggressionDuration());
        }

        public void ClearAggressionTimer()
        {
            _aggressionTimerTokenSource?.Cancel();
            _aggressionTimerTokenSource?.Dispose();
            _aggressionTimerTokenSource = null;
            _aggressionExpiresAtUtc = null;

            if (_rpStats != null)
                _rpStats.Aggression = 0;
        }

        public void SetBankAccount(UserRpBankAccount account)
        {
            _bankAccount = account;
        }

        public int GetReconnectRoomId()
        {
            return HomeRoomData?.roomid > 0 ? HomeRoomData.roomid : HomeRoom;
        }

        public void LoadHomeRoomData(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) {
                HomeRoomData = null;
                return;
            }

            try {
                HomeRoomData = JsonSerializer.Deserialize<HomeRoomDataState>(data);
                PendingReconnectPlacement = HomeRoomData?.roomid > 0;
            } catch {
                HomeRoomData = null;
                PendingReconnectPlacement = false;
            }
        }

        public string SerializeHomeRoomData()
        {
            if (HomeRoomData == null)
                return string.Empty;

            return JsonSerializer.Serialize(HomeRoomData);
        }

        public void CaptureHomeRoomData()
        {
            if (CurrentRoom == null)
                return;

            RoomUser roomUser = CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Id);
            if (roomUser == null)
                return;

            HomeRoomData = new HomeRoomDataState
            {
                roomid = CurrentRoomId,
                x = roomUser.X,
                y = roomUser.Y,
                z = roomUser.Z,
                rotation = roomUser.RotBody,
                weapon = GetRpWeapons()?.ActiveWeaponId ?? 0
            };
        }

        public void TrySendUserStatsUpdate(bool force = false)
        {
            if (GetClient() == null)
                return;

            double now = PlusEnvironment.GetUnixTimestamp();
            if (!force && LastUserStatsUpdate > now)
                return;

            // Include the currently clicked/locked target only while it is still in this room.
            Habbo target = TargetLockService.GetTarget(this);
            GetClient().SendPacket(new UserStatsComposer(GetClient(), target));
            LastUserStatsUpdate = now + 1;
        }

        public void ApplyHomeRoomData()
        {
            if (!PendingReconnectPlacement || HomeRoomData == null || CurrentRoom == null || CurrentRoomId != HomeRoomData.roomid)
                return;

            RoomUser roomUser = CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Id);
            if (roomUser == null)
                return;

            int targetX = HomeRoomData.x;
            int targetY = HomeRoomData.y;
            if (!CurrentRoom.GetGameMap().ValidTile(targetX, targetY) || !CurrentRoom.GetGameMap().SquareIsOpen(targetX, targetY, false)) {
                PendingReconnectPlacement = false;
                return;
            }

            Point oldPoint = new(roomUser.X, roomUser.Y);
            Point newPoint = new(targetX, targetY);

            CurrentRoom.GetGameMap().GameMap[roomUser.X, roomUser.Y] = roomUser.SqState;
            CurrentRoom.GetGameMap().UpdateUserMovement(oldPoint, newPoint, roomUser);
            roomUser.SqState = CurrentRoom.GetGameMap().GameMap[targetX, targetY];
            CurrentRoom.GetGameMap().GameMap[targetX, targetY] = 1;

            roomUser.SetPos(targetX, targetY, HomeRoomData.z);

            roomUser.RotBody = HomeRoomData.rotation;
            roomUser.RotHead = HomeRoomData.rotation;
            roomUser.UpdateNeeded = true;
            CurrentRoom.GetRoomUserManager().UpdateUserStatus(roomUser, false);

            // Resolve the RP effect the canonical way: when weapon == 0 this clears any equipped
            // weapon effect (properly unequipping), while still respecting staff/passive priority.
            if (GetRpWeapons() != null && GetRpWeapons().SetActiveWeaponId(HomeRoomData.weapon))
                Plus.HabboHotel.Roleplay.Utilities.RpEffectService.Refresh(this);

            PendingReconnectPlacement = false;
        }

        public int GetBubbleId(int requestedBubbleId)
        {
            int health = _rpStats?.Health ?? 100;

            if (_rpStats?.IsDead ?? false)
                return 32;

            if (health > 50)
                return CustomBubbleId == 0 ? requestedBubbleId : CustomBubbleId;

            if (health >= 20)
                return 5;

            return 3;
        }

        public bool HasChatBubble(int bubbleId)
        {
            return bubbleId == 0 || OwnedChatBubbleIds.Contains(bubbleId);
        }

        public int[] GetOwnedChatBubbleIds()
        {
            return OwnedChatBubbleIds.ToArray();
        }

        public UserRpSkills GetRpSkills()
        {
            return _rpSkills;
        }

        public UserRpWeapons GetRpWeapons()
        {
            return _rpWeapons;
        }

        public UserRpWeaponSkins GetRpWeaponSkins()
        {
            return _rpWeaponSkins;
        }

        public UserRpItems GetRpItems()
        {
            return _rpItems;
        }

        public UserCraftingRecipes GetCraftingRecipes()
        {
            return _craftingRecipes;
        }

        public int ProgressSkill(string progressCategory, int progress)
        {
            return _rpSkills.ProgressSkillByCategory(progressCategory, progress);
        }

        public int GetSkillLevel(int skillId)
        {
            return _rpSkills.GetLevel(skillId);
        }

        public bool HasSkill(int skillId)
        {
            return _rpSkills.HasSkill(skillId);
        }

        public bool HasUnlockedSkill(int skillId)
        {
            return _rpSkills.HasUnlockedSkill(skillId);
        }

        public bool ToggleSkill(int skillId)
        {
            if (skillId != 7)
                return _rpSkills.ToggleSkill(skillId);

            bool isEquipped = _rpSkills.IsEquipped(skillId);
            bool toggled = _rpSkills.ToggleSkill(skillId);
            if (!toggled || _rpStats == null)
                return toggled;

            if (isEquipped) {
                if (_rpStats.Health > 100) {
                    CachedSkillHealth = _rpStats.Health;
                    HasCachedSkillHealth = true;
                    _rpStats.Health = 100;
                }
            } else if (HasCachedSkillHealth) {
                if (CachedSkillHealth > _rpStats.Health)
                    _rpStats.Health = CachedSkillHealth;

                CachedSkillHealth = 0;
                HasCachedSkillHealth = false;
            }

            return true;
        }

        public void SaveRpStats()
        {
            if (_rpStats == null)
                return;

            try {
                using WavePlusContext db = PlusEnvironment.GetDbContext();
                db.UserRpStatistics.Upsert(new UserRpStatisticEntity
                {
                    UserId = Id,
                    Health = _rpStats.Health,
                    Shield = _rpStats.Shield,
                    Energy = _rpStats.Energy,
                    Hunger = _rpStats.Hunger,
                    Experience = _rpStats.Experience,
                    Knockouts = _rpStats.Knockouts,
                    Deaths = _rpStats.Deaths,
                    Arrested = _rpStats.Arrested,
                    Escapes = _rpStats.Escapes,
                    DamageDealt = _rpStats.DamageDealt,
                    DamageTaken = _rpStats.DamageTaken,
                    Attacks = _rpStats.Attacks,
                    Attacked = _rpStats.Attacked,
                    Knowledge = _rpStats.Knowledge,
                    Strength = _rpStats.Strength,
                    AttributePoints = _rpStats.AttributePoints,
                    IsDead = _rpStats.IsDead ? 1 : 0,
                    HospitalReleaseTime = _rpStats.HospitalReleaseTime,
                    HospitalHealStartTime = _rpStats.HospitalHealStartTime,
                    HospitalHealthStart = _rpStats.HospitalHealthStart,
                    Aggression = (short)GetAggressionSecondsRemaining(),
                    PassiveMode = (sbyte)(_rpStats.PassiveMode ? 1 : 0),
                    Robberies = _rpStats.Robberies,
                    Robbed = _rpStats.Robbed,
                    IsCuffed = _rpStats.IsCuffed,
                    JailReleaseTime = _rpStats.JailReleaseTime,
                    JailPending = _rpStats.JailPending,
                    JailStars = _rpStats.JailStars,
                    JailRevertLook = _rpStats.JailRevertLook ?? "",
                    JailRoomId = _rpStats.JailRoomId,
                    JailTimeLeft = _rpStats.JailTimeLeft
                })
                .On(s => s.UserId)
                .Run();
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }
        }

        public void SaveAllRpData()
        {
            SaveRpStats();
            SaveRpSkills();
            SaveRpWeapons();
            SaveRpItems();
            SaveRpWeaponSkins();
            SaveBank();
            SaveUserRow();
        }

        public void SaveUserRow()
        {
            try {
                // The dead respawn at the hospital — don't pin them to where they died.
                if (GetRpStats()?.IsDead != true)
                    CaptureHomeRoomData();

                string homeRoomData = SerializeHomeRoomData();
                bool friendBarState = FriendBarStateUtility.GetInt(FriendBarState) == 1;
                short bubble = (short)CustomBubbleId;

                using WavePlusContext db = PlusEnvironment.GetDbContext();

                db.Users.Where(u => u.Id == Id)
                    .ExecuteUpdate(s => s
                        .SetProperty(u => u.ActivityPoints, Duckets)
                        .SetProperty(u => u.Credits, Credits)
                        .SetProperty(u => u.VipPoints, Diamonds)
                        .SetProperty(u => u.HomeRoom, HomeRoom)
                        .SetProperty(u => u.HomeRoomData, homeRoomData)
                        .SetProperty(u => u.GotwPoints, GotwPoints)
                        .SetProperty(u => u.TimeMuted, TimeMuted)
                        .SetProperty(u => u.FriendBarState, friendBarState)
                        .SetProperty(u => u.Bubble, bubble));

                if (_habboStats == null)
                    return;

                // Recomputed from SessionStart against the loaded base each time, so running this
                // every cycle stays idempotent rather than compounding.
                int onlineTime = (int)(PlusEnvironment.GetUnixTimestamp() - SessionStart + _habboStats.OnlineTime);
                uint questId = (uint)_habboStats.QuestId;
                db.UserStats.Where(s => s.Id == Id)
                    .ExecuteUpdate(s => s
                        .SetProperty(x => x.RoomVisits, _habboStats.RoomVisits)
                        .SetProperty(x => x.OnlineTime, onlineTime)
                        .SetProperty(x => x.Respect, _habboStats.Respect)
                        .SetProperty(x => x.RespectGiven, _habboStats.RespectGiven)
                        .SetProperty(x => x.GiftsGiven, _habboStats.GiftsGiven)
                        .SetProperty(x => x.GiftsReceived, _habboStats.GiftsReceived)
                        .SetProperty(x => x.DailyRespectPoints, _habboStats.DailyRespectPoints)
                        .SetProperty(x => x.DailyPetRespectPoints, _habboStats.DailyPetRespectPoints)
                        .SetProperty(x => x.AchievementScore, _habboStats.AchievementPoints)
                        .SetProperty(x => x.QuestId, questId)
                        .SetProperty(x => x.QuestProgress, _habboStats.QuestProgress)
                        .SetProperty(x => x.Groupid, _habboStats.FavouriteGroupId)
                        .SetProperty(x => x.ForumPosts, _habboStats.ForumPosts));
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }
        }

        public void SaveRpSkills()
        {
            if (_rpSkills == null || !_rpSkills.Dirty)
                return;

            try {
                var rows = _rpSkills.Skills.Select(skill => new UserRpSkillEntity
                {
                    UserId = Id,
                    SkillId = skill.SkillId,
                    Progress = skill.Progress,
                    Equipped = skill.Equipped
                }).ToList();

                if (rows.Count > 0) {
                    using WavePlusContext db = PlusEnvironment.GetDbContext();
                    db.UserRpSkills.UpsertRange(rows)
                        .On(s => new { s.UserId, s.SkillId })
                        .Run();
                }

                _rpSkills.MarkSaved();
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }
        }

        public void SaveRpWeapons()
        {
            if (_rpWeapons == null || !_rpWeapons.Dirty)
                return;

            try {
                using WavePlusContext db = PlusEnvironment.GetDbContext();

                var removed = _rpWeapons.RemovedIds.ToList();
                if (removed.Count > 0)
                    db.UserRpWeapons.Where(w => removed.Contains(w.Id)).ExecuteDelete();

                // Track new rows so we can copy the DB-generated id back after SaveChanges.
                var inserted = new List<(UserWeapon Src, UserRpWeaponEntity Row)>();

                foreach (UserWeapon weapon in _rpWeapons.Weapons.ToList()) {
                    if (weapon.IsDefault)
                        continue;

                    if (weapon.Id > 0) {
                        var row = new UserRpWeaponEntity { Id = weapon.Id, DurabilityLeft = weapon.DurabilityLeft, Slot = weapon.Slot };
                        db.UserRpWeapons.Attach(row);
                        db.Entry(row).Property(w => w.DurabilityLeft).IsModified = true;
                        db.Entry(row).Property(w => w.Slot).IsModified = true;
                        continue;
                    }

                    var newRow = new UserRpWeaponEntity { UserId = Id, WeaponId = weapon.WeaponId, DurabilityLeft = weapon.DurabilityLeft, Slot = weapon.Slot };
                    db.UserRpWeapons.Add(newRow);
                    inserted.Add((weapon, newRow));
                }

                db.SaveChanges();

                foreach ((UserWeapon src, UserRpWeaponEntity row) in inserted) {
                    int oldId = src.Id;
                    src.Id = row.Id;
                    _rpWeapons.RekeyWeapon(oldId, src.Id);
                }

                _rpWeapons.MarkSaved();
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }
        }

        public void SaveRpItems()
        {
            if (_rpItems == null || !_rpItems.Dirty)
                return;

            try {
                using WavePlusContext db = PlusEnvironment.GetDbContext();

                var removed = _rpItems.RemovedIds.ToList();
                if (removed.Count > 0)
                    db.UserRpItems.Where(i => removed.Contains(i.Id)).ExecuteDelete();

                var inserted = new List<(UserRpItem Src, Plus.Database.EF.Entities.UserRpItemEntity Row)>();

                foreach (UserRpItem item in _rpItems.Items.ToList()) {
                    if (item.Id > 0) {
                        var row = new Plus.Database.EF.Entities.UserRpItemEntity { Id = item.Id, Uses = (short)item.Uses, Slot = item.Slot, DurabilityLeft = item.DurabilityLeft, Equipped = item.Equipped };
                        db.UserRpItems.Attach(row);
                        db.Entry(row).Property(i => i.Uses).IsModified = true;
                        db.Entry(row).Property(i => i.Slot).IsModified = true;
                        db.Entry(row).Property(i => i.DurabilityLeft).IsModified = true;
                        db.Entry(row).Property(i => i.Equipped).IsModified = true;
                        continue;
                    }

                    var newRow = new Plus.Database.EF.Entities.UserRpItemEntity { UserId = Id, ItemId = item.ItemId, Uses = (short)item.Uses, Slot = item.Slot, DurabilityLeft = item.DurabilityLeft, Equipped = item.Equipped };
                    db.UserRpItems.Add(newRow);
                    inserted.Add((item, newRow));
                }

                db.SaveChanges();

                foreach ((UserRpItem src, Plus.Database.EF.Entities.UserRpItemEntity row) in inserted) {
                    int oldId = src.Id;
                    src.Id = row.Id;
                    _rpItems.RekeyItem(oldId, src.Id);
                }

                _rpItems.MarkSaved();
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }
        }

        public void SaveRpWeaponSkins()
        {
            if (_rpWeaponSkins == null || !_rpWeaponSkins.Dirty)
                return;

            try {
                using WavePlusContext db = PlusEnvironment.GetDbContext();

                var inserted = new List<(UserWeaponSkin Src, UserRpWeaponSkinEntity Row)>();

                foreach (UserWeaponSkin skin in _rpWeaponSkins.Skins.ToList()) {
                    if (skin.Id > 0) {
                        // ExecuteUpdate rather than Attach: a row that has since been deleted just
                        // updates nothing, instead of failing the whole batch with a concurrency error.
                        int equipped = skin.Equipped ? 1 : 0;
                        db.UserRpWeaponSkins.Where(s => s.Id == skin.Id)
                            .ExecuteUpdate(s => s.SetProperty(x => x.Equipped, equipped));
                        continue;
                    }

                    var newRow = new UserRpWeaponSkinEntity { UserId = Id, SkinId = skin.SkinId, Equipped = skin.Equipped ? 1 : 0 };
                    db.UserRpWeaponSkins.Add(newRow);
                    inserted.Add((skin, newRow));
                }

                db.SaveChanges();

                foreach ((UserWeaponSkin src, UserRpWeaponSkinEntity row) in inserted) {
                    int oldId = src.Id;
                    src.Id = row.Id;
                    _rpWeaponSkins.RekeySkin(oldId, src.Id);
                }

                _rpWeaponSkins.MarkSaved();
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }
        }

        public void SaveBank()
        {
            PlusEnvironment.GetBankingManager().Save(this);
        }

        public bool InRoom => CurrentRoomId >= 1 && CurrentRoom != null;

        public Rooms.Room CurrentRoom
        {
            get
            {
                if (CurrentRoomId <= 0)
                    return null;

                if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(CurrentRoomId, out Room room))
                    return room;

                return null;
            }
        }

        public bool RoomEntryFinalized { get; set; }

        public bool CacheExpired()
        {
            TimeSpan span = DateTime.Now - _timeCached;
            return span.TotalMinutes >= 30;
        }

        public FormattableString GetQueryString
        {
            get
            {
                double lastOnline = PlusEnvironment.GetUnixTimestamp();
                double onlineTime = PlusEnvironment.GetUnixTimestamp() - SessionStart + _habboStats.OnlineTime;
                int friendBarState = FriendBarStateUtility.GetInt(FriendBarState);
                return $"UPDATE `users` SET `online` = '0', `last_online` = {lastOnline}, `activity_points` = {Duckets}, `credits` = {Credits}, `vip_points` = {Diamonds}, `home_room` = {HomeRoom}, `gotw_points` = {GotwPoints}, `time_muted` = {TimeMuted}, `friend_bar_state` = {friendBarState}, `bubble` = {CustomBubbleId} WHERE id = {Id} LIMIT 1;UPDATE `user_stats` SET `roomvisits` = {_habboStats.RoomVisits}, `onlineTime` = {onlineTime}, `respect` = {_habboStats.Respect}, `respectGiven` = {_habboStats.RespectGiven}, `giftsGiven` = {_habboStats.GiftsGiven}, `giftsReceived` = {_habboStats.GiftsReceived}, `dailyRespectPoints` = {_habboStats.DailyRespectPoints}, `dailyPetRespectPoints` = {_habboStats.DailyPetRespectPoints}, `AchievementScore` = {_habboStats.AchievementPoints}, `quest_id` = {_habboStats.QuestId}, `quest_progress` = {_habboStats.QuestProgress}, `groupid` = {_habboStats.FavouriteGroupId}, `forum_posts` = {_habboStats.ForumPosts} WHERE `id` = {Id} LIMIT 1;";
            }
        }

        public bool InitProcess()
        {
            _process = new ProcessComponent();

            return _process.Init(this);
        }

        public bool InitSearches()
        {
            _navigatorSearches = new SearchesComponent();

            return _navigatorSearches.Init(this);
        }

        public bool InitFx()
        {
            _fx = new EffectsComponent();

            return _fx.Init(this);
        }

        public bool InitClothing()
        {
            _clothing = new ClothingComponent();

            return _clothing.Init(this);
        }

        public bool InitIgnores()
        {
            _ignores = new IgnoresComponent();

            return _ignores.Init(this);
        }

        private bool InitPermissions()
        {
            _permissions = new PermissionComponent();

            return _permissions.Init(this);
        }

        public void InitInformation(UserData.UserData data)
        {
            _badgeComponent = new BadgeComponent(this, data);
            Relationships = data.Relations;
        }

        public void Init(GameClient client, UserData.UserData data)
        {
            Achievements = data.Achievements;

            FavoriteRooms = new ArrayList();
            foreach (int id in data.FavoritedRooms) {
                FavoriteRooms.Add(id);
            }

            _client = client;
            _badgeComponent = new BadgeComponent(this, data);
            _inventoryComponent = new InventoryComponent(Id, client);

            Quests = data.Quests;

            _messenger = new HabboMessenger(Id);
            _messenger.Init(data.Friends, data.Requests);
            FriendCount = Convert.ToInt32(data.Friends.Count);
            _disconnected = false;
            Relationships = data.Relations;

            InitSearches();
            InitFx();
            InitClothing();
            InitIgnores();

            // Resolve which corporation/business the user works for (if any).
            CorporationId = PlusEnvironment.GetGame().GetGroupManager().TryGetWorkGroupForUser(Id, out Groups.Group workGroup) ? workGroup.Id : 0;
        }

        public PermissionComponent GetPermissions()
        {
            return _permissions;
        }

        public IgnoresComponent GetIgnores()
        {
            return _ignores;
        }

        public void OnDisconnect()
        {
            if (_disconnected)
                return;

            RpItemClothingService.RestoreOnLogout(this);

            // End any active work shift: pay accrued bonus, restore look/motto, flush logs.
            PlusEnvironment.GetGame().GetShiftManager().OnDisconnect(this);

            // Drop cached web-overlay macros + target lock state.
            PlusEnvironment.GetGame().GetMacroManager().Unload(Id);
            TargetLockService.Clear(Id);
            GroupInviteService.Clear(Id);
            RpOfferService.Clear(Id);
            PlusEnvironment.GetPoliceManager()?.OnDisconnect(this);
            PlusEnvironment.GetJailManager()?.OnDisconnect(this);
            PlusEnvironment.GetTrashSearchManager()?.Cancel(Id);
            PlusEnvironment.GetCraftingManager()?.CancelCraft(this, notify: false);
            FurnitureAchievementService.Forget(Id);

            // Drop any half-finished arrow teleport state so the static maps can't accumulate
            // entries for users who logged out mid-transit.
            InteractorArrowTeleport.CancelDelayedTeleport(Id);
            InteractorArrowTeleport.ClearArrivalData(Id);

            // Close out the current room-visit record if one is still open.
            Plus.Core.Persistence.RoomVisitBuffer.OnExit(Id);

            // Persist any buffered crime logs (the wanted cache itself is never saved).
            PlusEnvironment.GetRpCrimeManager().FlushLogs(Id);

            if (GetRpStats()?.IsDead == true && GetRpStats().HospitalReleaseTime <= 0)
                PlusEnvironment.GetHospitalManager().SkipBleedout(this);

            try {
                if (GetRpStats()?.IsDead != true)
                    CaptureHomeRoomData();

                _process?.Dispose();
            } catch {
            }

            _disconnected = true;

            PlusEnvironment.GetGame().GetClientManager().UnregisterClient(Id, Username);

            if (!_habboSaved) {
                _habboSaved = true;
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    int lastOnline = (int)PlusEnvironment.GetUnixTimestamp();
                    db.Users.Where(u => u.Id == Id)
                        .ExecuteUpdate(s => s
                            .SetProperty(u => u.Online, "0")
                            .SetProperty(u => u.LastOnline, lastOnline));

                    if (GetPermissions().HasRight("mod_tickets")) {
                        uint modId = (uint)Id;
                        db.ModerationTickets.Where(t => t.Status == "picked" && t.ModeratorId == modId)
                            .ExecuteUpdate(s => s.SetProperty(t => t.Status, "open").SetProperty(t => t.ModeratorId, 0u));
                    }
                }

                SaveAllRpData();
            }

            // Reset this user's Redis stat mirror + active-set membership.
            Plus.Core.Cache.RpStatsCache.Remove(Id);

            Dispose();

            _client = null;
        }

        public void Dispose()
        {
            _inventoryComponent?.SetIdleState();

            if (InRoom && CurrentRoom != null)
                CurrentRoom.GetRoomUserManager().RemoveUserFromRoom(_client, false);

            if (_messenger != null) {
                _messenger.AppearOffline = true;
                _messenger.Destroy();
            }

            _fx?.Dispose();

            _clothing?.Dispose();

            _permissions?.Dispose();
            _aggressionTimerTokenSource?.Cancel();
            _aggressionTimerTokenSource?.Dispose();

            if (_ignores != null)
                _permissions.Dispose();
        }

        // Grants (or extends) timed VIP by the given number of seconds. Stacks on top of any
        // remaining time. Persists, refreshes command rights, re-validates the figure and pushes
        // the updated subscription info so the purse countdown reflects it immediately.
        public void GrantVip(int seconds)
        {
            if (seconds <= 0)
                return;

            int now = (int)PlusEnvironment.GetUnixTimestamp();
            int baseTime = VipExpire > now ? VipExpire : now;
            VipExpire = baseTime + seconds;
            VipRank = 1;

            PersistVip();
            OnVipChanged();
        }

        // Ends VIP immediately (expiry or manual removal): clears the flag, re-validates the figure
        // (strips club clothing) and refreshes command rights.
        public void ExpireVip()
        {
            VipExpire = 0;
            VipRank = 0;

            PersistVip();
            OnVipChanged();
        }

        private void PersistVip()
        {
            int id = Id;
            int expire = VipExpire;
            int rank = VipRank;

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Users.Where(u => u.Id == id)
                    .ExecuteUpdate(s => s.SetProperty(u => u.VipExpire, expire).SetProperty(u => u.RankVip, rank));
            }
        }

        private void OnVipChanged()
        {
            // Command/permission rights are keyed off VipRank; re-init so :commands updates live.
            GetPermissions().Init(this);

            // Re-validate the figure against the new VIP state. ProcessFigure strips any club-level
            // clothing parts when the user is no longer VIP (colors are left untouched).
            string processed = PlusEnvironment.GetFigureManager().ProcessFigure(Look, Gender, GetClothing().GetClothingParts, IsVip, GetPermissions().HasRight("clothing_no_validation"));

            if (processed != Look) {
                Look = processed;
                int id = Id;
                string look = processed;
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    db.Users.Where(u => u.Id == id).ExecuteUpdate(s => s.SetProperty(u => u.Look, look));
                }

                if (InRoom && CurrentRoom != null) {
                    RoomUser roomUser = CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Id);
                    if (roomUser != null) {
                        GetClient()?.SendPacket(new Plus.Communication.Packets.Outgoing.Rooms.Avatar.AvatarAspectUpdateComposer(processed, Gender));
                        CurrentRoom.SendPacket(new Plus.Communication.Packets.Outgoing.Rooms.Engine.UserChangeComposer(roomUser, false));
                    }
                }
            }

            // Push updated subscription info so the purse VIP countdown refreshes instantly.
            GetClient()?.SendPacket(new Plus.Communication.Packets.Outgoing.Users.ScrSendUserInfoComposer(this));
        }

        public void CheckCreditsTimer()
        {
            try {
                CreditsUpdateTick--;

                if (CreditsUpdateTick <= 0) {
                    int creditUpdate = Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("user.currency_scheduler.credit_reward"));
                    int ducketUpdate = Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("user.currency_scheduler.ducket_reward"));

                    if (PlusEnvironment.GetGame().GetSubscriptionManager().TryGetSubscriptionData(VipRank, out SubscriptionData subData)) {
                        creditUpdate += subData.Credits;
                        ducketUpdate += subData.Duckets;
                    }

                    Credits += creditUpdate;
                    Duckets += ducketUpdate;

                    _client.SendPacket(new CreditBalanceComposer(Credits));
                    _client.SendPacket(new HabboActivityPointNotificationComposer(Duckets, ducketUpdate));

                    CreditsUpdateTick = Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("user.currency_scheduler.tick"));
                }
            } catch {
            }
        }

        public void CheckBankSaveTimer()
        {
            try {
                BankSaveTick--;
                if (BankSaveTick > 0)
                    return;

                SaveBank();
                BankSaveTick = 900;
            } catch {
            }
        }

        private void StartAggressionTimer(int seconds)
        {
            ClearAggressionTimer();

            if (_rpStats == null || seconds <= 0)
                return;

            _rpStats.Aggression = seconds;
            _aggressionExpiresAtUtc = DateTime.UtcNow.AddSeconds(seconds);
            _aggressionTimerTokenSource = new CancellationTokenSource();
            _ = RunAggressionTimerAsync(seconds, _aggressionTimerTokenSource.Token);
        }

        private async Task RunAggressionTimerAsync(int seconds, CancellationToken cancellationToken)
        {
            try {
                while (!cancellationToken.IsCancellationRequested) {
                    SyncAggressionState();
                    if ((_rpStats?.Aggression ?? 0) <= 0) {
                        ClearAggressionTimer();
                        TrySendUserStatsUpdate(true);
                        return;
                    }

                    TrySendUserStatsUpdate(true);

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
            } catch (TaskCanceledException) {
            }
        }

        private void SyncAggressionState()
        {
            if (_rpStats == null)
                return;

            int secondsRemaining = CalculateAggressionSecondsRemaining();
            _rpStats.Aggression = secondsRemaining;

            if (secondsRemaining > 0 || _aggressionExpiresAtUtc.HasValue)
                return;

            _aggressionTimerTokenSource?.Cancel();
            _aggressionTimerTokenSource?.Dispose();
            _aggressionTimerTokenSource = null;
        }

        private int CalculateAggressionSecondsRemaining()
        {
            if (!_aggressionExpiresAtUtc.HasValue)
                return 0;

            double secondsRemaining = (_aggressionExpiresAtUtc.Value - DateTime.UtcNow).TotalSeconds;
            return secondsRemaining <= 0 ? 0 : (int)Math.Ceiling(secondsRemaining);
        }

        public GameClient GetClient()
        {
            if (_client != null)
                return _client;

            return PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(Id);
        }

        public HabboMessenger GetMessenger()
        {
            return _messenger;
        }

        public BadgeComponent GetBadgeComponent()
        {
            return _badgeComponent;
        }

        public InventoryComponent GetInventoryComponent()
        {
            return _inventoryComponent;
        }

        public SearchesComponent GetNavigatorSearches()
        {
            return _navigatorSearches;
        }

        public EffectsComponent Effects()
        {
            return _fx;
        }

        public ClothingComponent GetClothing()
        {
            return _clothing;
        }

        public int GetQuestProgress(int p)
        {
            Quests.TryGetValue(p, out int progress);
            return progress;
        }

        public UserAchievement GetAchievementData(string p)
        {
            Achievements.TryGetValue(p, out UserAchievement achievement);
            return achievement;
        }

        public void ChangeName(string username)
        {
            LastNameChange = PlusEnvironment.GetUnixTimestamp();
            Username = username;

            SaveKey("username", username);
            SaveKey("last_change", LastNameChange.ToString());
        }

        public void SaveKey(string key, string value)
        {
            using WavePlusContext db = PlusEnvironment.GetDbContext();
#pragma warning disable EF1003 // Risk of SQL injection — only the trusted internal column name is concatenated.
            db.Database.ExecuteSqlRaw("UPDATE `users` SET " + key + " = {0} WHERE `id` = {1} LIMIT 1;", value, Id);
#pragma warning restore EF1003
        }

        public void PrepareRoom(int id, string password)
        {
            GameClient client = GetClient();
            if (client == null || client.GetHabbo() == null)
                return;

            Habbo habbo = client.GetHabbo();

            if (habbo.InRoom) {
                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(habbo.CurrentRoomId, out Rooms.Room oldRoom)) {
                    // Old room no longer loaded (unloaded while idle) — clear the stale room ID and continue.
                    habbo.CurrentRoomId = 0;
                } else if (oldRoom.GetRoomUserManager() != null) {
                    oldRoom.GetRoomUserManager().RemoveUserFromRoom(client, false);
                }
            }

            if (habbo.IsTeleporting && habbo.TeleportingRoomId != id) {
                client.SendPacket(new CloseConnectionComposer());
                return;
            }

            if (!PlusEnvironment.GetGame().GetRoomManager().TryLoadRoom(id, out Room room)) {
                client.SendPacket(new CloseConnectionComposer());
                return;
            }

            if (room.IsCrashed) {
                client.SendNotification("This room has crashed! :(");
                client.SendPacket(new CloseConnectionComposer());
                return;
            }

            habbo.CurrentRoomId = room.RoomId;

            if (room.GetRoomUserManager().UserCount >= room.UsersMax && !habbo.GetPermissions().HasRight("room_enter_full") && habbo.Id != room.OwnerId) {
                client.SendPacket(new CantConnectComposer(1));
                client.SendPacket(new CloseConnectionComposer());
                return;
            }

            if (!habbo.GetPermissions().HasRight("room_ban_override") && room.GetBans().IsBanned(habbo.Id)) {
                habbo.RoomAuthOk = false;
                client.SendPacket(new CantConnectComposer(4));
                client.SendPacket(new CloseConnectionComposer());
                return;
            }

            client.SendPacket(new OpenConnectionComposer());
            if (!room.CheckRights(client, true, true) && !habbo.IsTeleporting && !habbo.IsHopping) {
                if (room.Access == RoomAccess.Doorbell && !habbo.GetPermissions().HasRight("room_enter_locked")) {
                    if (room.UserCount > 0) {
                        client.SendPacket(new DoorbellComposer(""));
                        room.SendPacket(new DoorbellComposer(habbo.Username), true);
                        return;
                    }

                    client.SendPacket(new FlatAccessDeniedComposer(""));
                    client.SendPacket(new CloseConnectionComposer());
                    return;
                }

                if (room.Access == RoomAccess.Password && !habbo.GetPermissions().HasRight("room_enter_locked")) {
                    if (password.ToLower() != room.Password.ToLower() || string.IsNullOrWhiteSpace(password)) {
                        client.SendPacket(new GenericErrorComposer(-100002));
                        client.SendPacket(new CloseConnectionComposer());
                        return;
                    }
                }
            }

            if (!EnterRoom(room)) {
                client.SendPacket(new CloseConnectionComposer());
            }
        }

        public bool EnterRoom(Room room)
        {
            GameClient client = GetClient();
            if (client == null || client.GetHabbo() == null)
                return false;

            if (room == null) {
                client.SendPacket(new CloseConnectionComposer());
                return false;
            }

            if (!RoomFactory.TryGetData(room.Id, out RoomData data)) {
                return false;
            }

            Habbo habbo = client.GetHabbo();

            habbo.RoomEntryFinalized = false;

            data.UsersNow++;
            client.SendPacket(new RoomReadyComposer(room));
            room.SendObjects(this.GetClient(), room);

            if (room.Wallpaper != "0.0")
                client.SendPacket(new RoomPropertyComposer("wallpaper", room.Wallpaper));
            if (room.Floor != "0.0")
                client.SendPacket(new RoomPropertyComposer("floor", room.Floor));

            client.SendPacket(new RoomPropertyComposer("landscape", room.Landscape));
            client.SendPacket(new RoomRatingComposer(room.Score, !(habbo.RatedRooms.Contains(room.RoomId) || room.OwnerId == habbo.Id)));

            // need to send it regardless cus nitro expects it
            // client.SendPacket(new FurnitureAliasesComposer());

            // Buffered (write-behind) — room entry no longer runs a synchronous INSERT.
            Plus.Core.Persistence.RoomVisitBuffer.OnEnter(habbo.Id, habbo.CurrentRoomId);

            if (room.OwnerId != habbo.Id) {
                habbo.GetStats().RoomVisits += 1;
                PlusEnvironment.GetGame().GetAchievementManager().QueueProgress(client, "ACH_RoomEntry", 1);
            }

            if (PlusEnvironment.GetSettingsManager().TryGetValue("room.entry.proactive") == "1") {
                habbo.RoomEntryFinalized = true;
                GetRoomEntryDataEvent.Finalize(client);
            }

            return true;
        }
    }
}