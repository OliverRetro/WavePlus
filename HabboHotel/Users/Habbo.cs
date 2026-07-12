using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Core;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Microsoft.EntityFrameworkCore;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboHotel.Roleplay.Banking;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Roleplay.RpItem.Item;
using Plus.HabboHotel.Roleplay.Skill;
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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Plus.HabboHotel.Roleplay.TargetLock;
using Plus.Communication.Packets.Outgoing.Quests;

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
        public double LastHitTime { get; set; }
        public double LastRobTime { get; set; }
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
        private UserRpBankAccount _bankAccount;
        private CancellationTokenSource _aggressionTimerTokenSource;
        private DateTime? _aggressionExpiresAtUtc;

        public double CustomBuildHeight { get; set; }

        public Habbo(int id, string username, int rank, string motto, string look, string gender, int credits, int activityPoints, int homeRoom,
            bool hasFriendRequestsDisabled, int lastOnline, bool appearOffline, bool hideInRoom, double createDate, int diamonds,
            string machineId, string clientVolume, bool chatPreference, bool focusPreference, bool petsMuted, bool botsMuted, bool advertisingReportBlocked, double lastNameChange,
            int gotwPoints, bool ignoreInvites, double timeMuted, double tradingLock, bool allowGifts, int friendBarState, bool disableForcedEffects, bool allowMimic, int vipRank, int bubble)
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
            foreach (string str in clientVolume.Split(','))
            {
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

            _disconnected = false;
            _habboSaved = false;
            ChangingName = false;

            FloodTime = 0;
            FriendCount = 0;
            TimeMuted = timeMuted;
            _timeCached = DateTime.Now;

            TradingLockExpiry = tradingLock;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                if (!db.user_infos.Any(u => u.user_id == id)) {
                    db.user_infos.Add(new user_info { user_id = id });
                    db.SaveChanges();
                }

                if (TradingLockExpiry > 0 && PlusEnvironment.GetUnixTimestamp() > TradingLockExpiry) {
                    TradingLockExpiry = 0;
                    db.user_infos.Where(u => u.user_id == id)
                        .ExecuteUpdate(s => s.SetProperty(u => u.trading_locked, 0.0));
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

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                user_stat statRow = db.user_stats.FirstOrDefault(s => s.id == id);
                if (statRow == null) {
                    statRow = new user_stat { id = id };
                    db.user_stats.Add(statRow);
                    db.SaveChanges();
                }

                try
                {
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
                        (int)statRow.quest_id,
                        statRow.quest_progress,
                        statRow.groupid,
                        statRow.respectsTimestamp,
                        statRow.forum_posts
                    );

                    if (statRow.respectsTimestamp != DateTime.Today.ToString("MM/dd")) {
                        _habboStats.RespectsTimestamp = DateTime.Today.ToString("MM/dd");

                        int dailyRespects = 10;

                        if (_permissions.HasRight("mod_tool"))
                            dailyRespects = 20;
                        else if (PlusEnvironment.GetGame().GetSubscriptionManager().TryGetSubscriptionData(vipRank, out SubscriptionData subData))
                            dailyRespects = subData.Respects;

                        _habboStats.DailyRespectPoints = dailyRespects;
                        _habboStats.DailyPetRespectPoints = dailyRespects;

                        string respectsTimestamp = DateTime.Today.ToString("MM/dd");
                        db.user_stats.Where(s => s.id == id)
                            .ExecuteUpdate(s => s
                                .SetProperty(x => x.DailyRespectPoints, dailyRespects)
                                .SetProperty(x => x.DailyPetRespectPoints, dailyRespects)
                                .SetProperty(x => x.respectsTimestamp, respectsTimestamp));
                    }
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                }

                List<int> chatBubbleIds = db.user_chat_bubbles.Where(b => b.user_id == id).Select(b => b.chat_bubble_id).ToList();
                foreach (int chatBubbleId in chatBubbleIds)
                {
                    if (!OwnedChatBubbleIds.Contains(chatBubbleId))
                        OwnedChatBubbleIds.Add(chatBubbleId);
                }

                OwnedChatBubbleIds.Add(0);
            }

            _bankAccount = PlusEnvironment.GetBankingManager().LoadAccount(id);

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(_habboStats.FavouriteGroupId, out Group _))
                _habboStats.FavouriteGroupId = 0;

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                user_rp_statistic statRow = db.user_rp_statistics.FirstOrDefault(s => s.user_id == id);
                if (statRow == null) {
                    statRow = new user_rp_statistic { user_id = id };
                    db.user_rp_statistics.Add(statRow);
                    db.SaveChanges();
                }

                try
                {
                    _rpStats = new UserRpStats(
                        id,
                        statRow.health,
                        statRow.shield,
                        statRow.energy,
                        statRow.hunger,
                        statRow.experience,
                        statRow.knockouts,
                        statRow.deaths,
                        statRow.arrested,
                        statRow.escapes,
                        (int)statRow.damage_dealt,
                        (int)statRow.damage_taken,
                        statRow.attacks,
                        statRow.attacked,
                        statRow.knowledge,
                        statRow.strength,
                        statRow.is_dead == 1,
                        statRow.hospital_release_time,
                        statRow.hospital_heal_start_time,
                        statRow.hospital_health_start,
                        statRow.aggression,
                        statRow.passive_mode == 1,
                        statRow.robberies,
                        statRow.robbed,
                        statRow.is_cuffed,
                        statRow.jail_release_time,
                        statRow.jail_pending,
                        statRow.jail_stars,
                        statRow.jail_revert_look ?? "",
                        statRow.jail_room_id,
                        statRow.jail_time_left
                    );
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                }
            }

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var weaponRows = db.user_rp_weapons.Where(w => w.user_id == id)
                    .Select(w => new { w.id, w.weapon_id, w.durability_left, w.slot }).ToList();

                List<UserWeapon> loadedWeapons = new();
                foreach (var row in weaponRows)
                {
                    Weapon weaponData = PlusEnvironment.GetWeaponManager().GetWeaponById(row.weapon_id);
                    if (weaponData == null)
                        continue;

                    loadedWeapons.Add(new UserWeapon(row.id, id, row.weapon_id, row.durability_left, weaponData, false, row.slot));
                }

                _rpWeapons = new UserRpWeapons(id, PlusEnvironment.GetWeaponManager(), loadedWeapons);
                _rpWeapons.MarkSaved();
                _rpStats.Weapons = _rpWeapons;
            }

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var skinRows = db.user_rp_weapon_skins.Where(s => s.user_id == id)
                    .Select(s => new { s.id, s.skin_id, s.equipped }).ToList();

                List<UserWeaponSkin> loadedSkins = new();
                foreach (var row in skinRows)
                {
                    WeaponSkin skinData = PlusEnvironment.GetWeaponManager().GetSkinById(row.skin_id);
                    if (skinData == null)
                        continue;

                    loadedSkins.Add(new UserWeaponSkin(row.id, id, skinData.Id, row.equipped == 1, skinData));
                }

                _rpWeaponSkins = new UserRpWeaponSkins(loadedSkins);
                _rpWeaponSkins.MarkSaved();
            }

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var skillRows = db.user_rp_skills.Where(s => s.user_id == id)
                    .Select(s => new { s.skill_id, s.progress, s.equipped }).ToList();

                List<UserSkillData> loadedSkills = new();
                foreach (var row in skillRows)
                {
                    loadedSkills.Add(new UserSkillData(row.skill_id, row.progress, row.equipped));
                }

                _rpSkills = new UserRpSkills(PlusEnvironment.GetSkillManager(), loadedSkills);
                _rpSkills.MarkSaved();
            }

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var itemRows = db.user_rp_items.Where(i => i.user_id == id)
                    .Select(i => new { i.id, i.item_id, i.uses, i.slot, i.durability_left, i.equipped }).ToList();

                List<UserRpItem> loadedItems = new();
                foreach (var row in itemRows)
                {
                    RpItemData itemData = PlusEnvironment.GetRpItemManager().GetItemById(row.item_id);
                    if (itemData == null)
                        continue;

                    loadedItems.Add(new UserRpItem(row.id, id, row.item_id, row.uses, itemData, row.slot, row.durability_left, row.equipped));
                }

                _rpItems = new UserRpItems(id, PlusEnvironment.GetRpItemManager(), loadedItems);
                _rpItems.MarkSaved();

                // Restore the equipped shield's durability into the live shield stat.
                UserRpItem equippedShield = _rpItems.GetEquippedShield();
                if (equippedShield != null && _rpStats != null)
                    _rpStats.Shield = equippedShield.EffectiveDurability;
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

        public int GetAggressionSecondsRemaining()
        {
            SyncAggressionState();
            return _rpStats?.Aggression ?? 0;
        }

        public void ResetAggressionTimer()
        {
            int aggressionDuration = 30;
            if (PlusEnvironment.GetGame().GetSubscriptionManager().TryGetSubscriptionData(VipRank, out SubscriptionData subData))
                aggressionDuration = Math.Max(0, aggressionDuration - subData.StaticAggression);

            StartAggressionTimer(aggressionDuration);
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

            try
            {
                HomeRoomData = JsonSerializer.Deserialize<HomeRoomDataState>(data);
                PendingReconnectPlacement = HomeRoomData?.roomid > 0;
            }
            catch
            {
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
            roomUser.SetRot(HomeRoomData.rotation, false);
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

            try
            {
                using WavePlusContext db = PlusEnvironment.GetDbContext();
                db.user_rp_statistics.Upsert(new user_rp_statistic
                {
                    user_id = Id,
                    health = _rpStats.Health,
                    shield = _rpStats.Shield,
                    energy = _rpStats.Energy,
                    hunger = _rpStats.Hunger,
                    experience = _rpStats.Experience,
                    knockouts = _rpStats.Knockouts,
                    deaths = _rpStats.Deaths,
                    arrested = _rpStats.Arrested,
                    escapes = _rpStats.Escapes,
                    damage_dealt = _rpStats.DamageDealt,
                    damage_taken = _rpStats.DamageTaken,
                    attacks = _rpStats.Attacks,
                    attacked = _rpStats.Attacked,
                    knowledge = _rpStats.Knowledge,
                    strength = _rpStats.Strength,
                    is_dead = _rpStats.IsDead ? 1 : 0,
                    hospital_release_time = _rpStats.HospitalReleaseTime,
                    hospital_heal_start_time = _rpStats.HospitalHealStartTime,
                    hospital_health_start = _rpStats.HospitalHealthStart,
                    aggression = (short)GetAggressionSecondsRemaining(),
                    passive_mode = (sbyte)(_rpStats.PassiveMode ? 1 : 0),
                    robberies = _rpStats.Robberies,
                    robbed = _rpStats.Robbed,
                    is_cuffed = _rpStats.IsCuffed,
                    jail_release_time = _rpStats.JailReleaseTime,
                    jail_pending = _rpStats.JailPending,
                    jail_stars = _rpStats.JailStars,
                    jail_revert_look = _rpStats.JailRevertLook ?? "",
                    jail_room_id = _rpStats.JailRoomId,
                    jail_time_left = _rpStats.JailTimeLeft
                })
                .On(s => s.user_id)
                .Run();
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        /// <summary>
        /// Flushes every piece of this user's roleplay data to the database. Called by the
        /// 5-minute persistence saver and on disconnect. The individual saves are dirty-gated
        /// where applicable, so calling this for all online users each cycle is cheap.
        /// </summary>
        public void SaveAllRpData()
        {
            SaveRpStats();
            SaveRpSkills();
            SaveRpWeapons();
            SaveRpItems();
            SaveRpWeaponSkins();
            SaveBank();
        }

        public void SaveRpSkills()
        {
            if (_rpSkills == null || !_rpSkills.Dirty)
                return;

            try
            {
                var rows = _rpSkills.Skills.Select(skill => new user_rp_skill
                {
                    user_id = Id,
                    skill_id = skill.SkillId,
                    progress = skill.Progress,
                    equipped = skill.Equipped
                }).ToList();

                if (rows.Count > 0)
                {
                    using WavePlusContext db = PlusEnvironment.GetDbContext();
                    db.user_rp_skills.UpsertRange(rows)
                        .On(s => new { s.user_id, s.skill_id })
                        .Run();
                }

                _rpSkills.MarkSaved();
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        public void SaveRpWeapons()
        {
            if (_rpWeapons == null || !_rpWeapons.Dirty)
                return;

            try
            {
                using WavePlusContext db = PlusEnvironment.GetDbContext();

                var removed = _rpWeapons.RemovedIds.ToList();
                if (removed.Count > 0)
                    db.user_rp_weapons.Where(w => removed.Contains(w.id)).ExecuteDelete();

                // Track new rows so we can copy the DB-generated id back after SaveChanges.
                var inserted = new List<(UserWeapon Src, user_rp_weapon Row)>();

                foreach (UserWeapon weapon in _rpWeapons.Weapons.ToList())
                {
                    if (weapon.IsDefault)
                        continue;

                    if (weapon.Id > 0) {
                        var row = new user_rp_weapon { id = weapon.Id, durability_left = weapon.DurabilityLeft, slot = weapon.Slot };
                        db.user_rp_weapons.Attach(row);
                        db.Entry(row).Property(w => w.durability_left).IsModified = true;
                        db.Entry(row).Property(w => w.slot).IsModified = true;
                        continue;
                    }

                    var newRow = new user_rp_weapon { user_id = Id, weapon_id = weapon.WeaponId, durability_left = weapon.DurabilityLeft, slot = weapon.Slot };
                    db.user_rp_weapons.Add(newRow);
                    inserted.Add((weapon, newRow));
                }

                db.SaveChanges();

                foreach ((UserWeapon src, user_rp_weapon row) in inserted)
                {
                    int oldId = src.Id;
                    src.Id = row.id;
                    _rpWeapons.RekeyWeapon(oldId, src.Id);
                }

                _rpWeapons.MarkSaved();
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        public void SaveRpItems()
        {
            if (_rpItems == null || !_rpItems.Dirty)
                return;

            try
            {
                using WavePlusContext db = PlusEnvironment.GetDbContext();

                var removed = _rpItems.RemovedIds.ToList();
                if (removed.Count > 0)
                    db.user_rp_items.Where(i => removed.Contains(i.id)).ExecuteDelete();

                var inserted = new List<(UserRpItem Src, user_rp_item Row)>();

                foreach (UserRpItem item in _rpItems.Items.ToList())
                {
                    if (item.Id > 0) {
                        var row = new user_rp_item { id = item.Id, uses = (short)item.Uses, slot = item.Slot, durability_left = item.DurabilityLeft, equipped = item.Equipped };
                        db.user_rp_items.Attach(row);
                        db.Entry(row).Property(i => i.uses).IsModified = true;
                        db.Entry(row).Property(i => i.slot).IsModified = true;
                        db.Entry(row).Property(i => i.durability_left).IsModified = true;
                        db.Entry(row).Property(i => i.equipped).IsModified = true;
                        continue;
                    }

                    var newRow = new user_rp_item { user_id = Id, item_id = item.ItemId, uses = (short)item.Uses, slot = item.Slot, durability_left = item.DurabilityLeft, equipped = item.Equipped };
                    db.user_rp_items.Add(newRow);
                    inserted.Add((item, newRow));
                }

                db.SaveChanges();

                foreach ((UserRpItem src, user_rp_item row) in inserted)
                {
                    int oldId = src.Id;
                    src.Id = row.id;
                    _rpItems.RekeyItem(oldId, src.Id);
                }

                _rpItems.MarkSaved();
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        public void SaveRpWeaponSkins()
        {
            if (_rpWeaponSkins == null || !_rpWeaponSkins.Dirty)
                return;

            try
            {
                using WavePlusContext db = PlusEnvironment.GetDbContext();

                var inserted = new List<(UserWeaponSkin Src, user_rp_weapon_skin Row)>();

                foreach (UserWeaponSkin skin in _rpWeaponSkins.Skins)
                {
                    if (skin.Id > 0) {
                        var row = new user_rp_weapon_skin { id = skin.Id, equipped = skin.Equipped ? 1 : 0 };
                        db.user_rp_weapon_skins.Attach(row);
                        db.Entry(row).Property(s => s.equipped).IsModified = true;
                        continue;
                    }

                    var newRow = new user_rp_weapon_skin { user_id = Id, skin_id = skin.SkinId, equipped = skin.Equipped ? 1 : 0 };
                    db.user_rp_weapon_skins.Add(newRow);
                    inserted.Add((skin, newRow));
                }

                db.SaveChanges();

                foreach ((UserWeaponSkin src, user_rp_weapon_skin row) in inserted)
                {
                    int oldId = src.Id;
                    src.Id = row.id;
                    _rpWeaponSkins.RekeySkin(oldId, src.Id);
                }

                _rpWeaponSkins.MarkSaved();
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        public void SaveBank()
        {
            PlusEnvironment.GetBankingManager().Save(this);
        }

        public bool InRoom => CurrentRoomId >= 1 && CurrentRoom != null;

        public Room CurrentRoom
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

        public bool CacheExpired()
        {
            TimeSpan span = DateTime.Now - _timeCached;
            return span.TotalMinutes >= 30;
        }

        public string GetQueryString
        {
            get
            {
                return "UPDATE `users` SET `online` = '0', `last_online` = '" + PlusEnvironment.GetUnixTimestamp() + "', `activity_points` = '" + Duckets + "', `credits` = '" + Credits + "', `vip_points` = '" + Diamonds + "', `home_room` = '" + HomeRoom + "', `gotw_points` = '" + GotwPoints + "', `time_muted` = '" + TimeMuted + "',`friend_bar_state` = '" + FriendBarStateUtility.GetInt(FriendBarState) + "',`bubble` = '" + CustomBubbleId + "' WHERE id = '" + Id + "' LIMIT 1;UPDATE `user_stats` SET `roomvisits` = '" + _habboStats.RoomVisits + "', `onlineTime` = '" + (PlusEnvironment.GetUnixTimestamp() - SessionStart + _habboStats.OnlineTime) + "', `respect` = '" + _habboStats.Respect + "', `respectGiven` = '" + _habboStats.RespectGiven + "', `giftsGiven` = '" + _habboStats.GiftsGiven + "', `giftsReceived` = '" + _habboStats.GiftsReceived + "', `dailyRespectPoints` = '" + _habboStats.DailyRespectPoints + "', `dailyPetRespectPoints` = '" + _habboStats.DailyPetRespectPoints + "', `AchievementScore` = '" + _habboStats.AchievementPoints + "', `quest_id` = '" + _habboStats.QuestId + "', `quest_progress` = '" + _habboStats.QuestProgress + "', `groupid` = '" + _habboStats.FavouriteGroupId + "',`forum_posts` = '" + _habboStats.ForumPosts + "' WHERE `id` = '" + Id + "' LIMIT 1;";
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
            foreach (int id in data.FavoritedRooms)
            {
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
            CorporationId = PlusEnvironment.GetGame().GetGroupManager().TryGetWorkGroupForUser(Id, out Group workGroup) ? workGroup.Id : 0;
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

            // End any active work shift: pay accrued bonus, restore look/motto, flush logs.
            PlusEnvironment.GetGame().GetShiftManager().OnDisconnect(this);

            // Drop cached web-overlay macros + target lock state.
            PlusEnvironment.GetGame().GetMacroManager().Unload(Id);
            Plus.HabboHotel.Roleplay.TargetLock.TargetLockService.Clear(Id);
            PlusEnvironment.GetPoliceManager()?.OnDisconnect(this);
            PlusEnvironment.GetJailManager()?.OnDisconnect(this);
            PlusEnvironment.GetTrashSearchManager()?.Cancel(Id);
            Plus.HabboHotel.Roleplay.Utilities.FurnitureAchievementService.Forget(Id);

            // Persist any buffered crime logs (the wanted cache itself is never saved).
            PlusEnvironment.GetRpCrimeManager().FlushLogs(Id);

            if (GetRpStats()?.IsDead == true && GetRpStats().HospitalReleaseTime <= 0)
                PlusEnvironment.GetHospitalManager().SkipBleedout(this);

            try
            {
                if (GetRpStats()?.IsDead != true)
                    CaptureHomeRoomData();

                _process?.Dispose();
            }
            catch
            {
            }

            _disconnected = true;

            PlusEnvironment.GetGame().GetClientManager().UnregisterClient(Id, Username);

            if (!_habboSaved) {
                _habboSaved = true;
                using (WavePlusContext db = PlusEnvironment.GetDbContext())
                {
                    int lastOnline = (int)PlusEnvironment.GetUnixTimestamp();
                    string homeRoomData = SerializeHomeRoomData();
                    bool friendBarState = FriendBarStateUtility.GetInt(FriendBarState) == 1;
                    short bubble = (short)CustomBubbleId;
                    db.users.Where(u => u.id == Id)
                        .ExecuteUpdate(s => s
                            .SetProperty(u => u.online, "0")
                            .SetProperty(u => u.last_online, lastOnline)
                            .SetProperty(u => u.activity_points, Duckets)
                            .SetProperty(u => u.credits, Credits)
                            .SetProperty(u => u.vip_points, Diamonds)
                            .SetProperty(u => u.home_room, HomeRoom)
                            .SetProperty(u => u.home_room_data, homeRoomData)
                            .SetProperty(u => u.gotw_points, GotwPoints)
                            .SetProperty(u => u.time_muted, TimeMuted)
                            .SetProperty(u => u.friend_bar_state, friendBarState)
                            .SetProperty(u => u.bubble, bubble));

                    int onlineTime = (int)(PlusEnvironment.GetUnixTimestamp() - SessionStart + _habboStats.OnlineTime);
                    uint questId = (uint)_habboStats.QuestId;
                    db.user_stats.Where(s => s.id == Id)
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
                            .SetProperty(x => x.quest_id, questId)
                            .SetProperty(x => x.quest_progress, _habboStats.QuestProgress)
                            .SetProperty(x => x.groupid, _habboStats.FavouriteGroupId)
                            .SetProperty(x => x.forum_posts, _habboStats.ForumPosts));

                    if (GetPermissions().HasRight("mod_tickets")) {
                        uint modId = (uint)Id;
                        db.moderation_tickets.Where(t => t.status == "picked" && t.moderator_id == modId)
                            .ExecuteUpdate(s => s.SetProperty(t => t.status, "open").SetProperty(t => t.moderator_id, 0u));
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

        public void CheckCreditsTimer()
        {
            try
            {
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
            }
            catch
            {
            }
        }

        public void CheckBankSaveTimer()
        {
            try
            {
                BankSaveTick--;
                if (BankSaveTick > 0)
                    return;

                SaveBank();
                BankSaveTick = 900;
            }
            catch
            {
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
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    SyncAggressionState();
                    if ((_rpStats?.Aggression ?? 0) <= 0) {
                        ClearAggressionTimer();
                        return;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
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
            // NOTE: the target column name is dynamic (caller-supplied), which EF's strongly-typed
            // ExecuteUpdate cannot express, so a parameterized raw update is retained. The value is
            // bound as a parameter ({0}); `key` and Id are trusted internal values.
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            db.Database.ExecuteSqlRaw("UPDATE `users` SET " + key + " = {0} WHERE `id` = " + Id + " LIMIT 1;", value);
        }

        public void PrepareRoom(int id, string password)
        {
            if (GetClient() == null || GetClient().GetHabbo() == null)
                return;

            if (GetClient().GetHabbo().InRoom) {
                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(GetClient().GetHabbo().CurrentRoomId, out Room oldRoom)) {
                    // Old room no longer loaded (unloaded while idle) — clear the stale room ID and continue.
                    GetClient().GetHabbo().CurrentRoomId = 0;
                } else if (oldRoom.GetRoomUserManager() != null) {
                    oldRoom.GetRoomUserManager().RemoveUserFromRoom(GetClient(), false);
                }
            }

            if (GetClient().GetHabbo().IsTeleporting && GetClient().GetHabbo().TeleportingRoomId != id) {
                GetClient().SendPacket(new CloseConnectionComposer());
                return;
            }

            if (!PlusEnvironment.GetGame().GetRoomManager().TryLoadRoom(id, out Room room)) {
                GetClient().SendPacket(new CloseConnectionComposer());
                return;
            }

            if (room.IsCrashed) {
                GetClient().SendNotification("This room has crashed! :(");
                GetClient().SendPacket(new CloseConnectionComposer());
                return;
            }

            GetClient().GetHabbo().CurrentRoomId = room.RoomId;

            if (room.GetRoomUserManager().UserCount >= room.UsersMax && !GetClient().GetHabbo().GetPermissions().HasRight("room_enter_full") && GetClient().GetHabbo().Id != room.OwnerId) {
                GetClient().SendPacket(new CantConnectComposer(1));
                GetClient().SendPacket(new CloseConnectionComposer());
                return;
            }


            if (!GetPermissions().HasRight("room_ban_override") && room.GetBans().IsBanned(Id)) {
                RoomAuthOk = false;
                GetClient().GetHabbo().RoomAuthOk = false;
                GetClient().SendPacket(new CantConnectComposer(4));
                GetClient().SendPacket(new CloseConnectionComposer());
                return;
            }

            GetClient().SendPacket(new OpenConnectionComposer());
            if (!room.CheckRights(GetClient(), true, true) && !GetClient().GetHabbo().IsTeleporting && !GetClient().GetHabbo().IsHopping) {
                if (room.Access == RoomAccess.Doorbell && !GetClient().GetHabbo().GetPermissions().HasRight("room_enter_locked")) {
                    if (room.UserCount > 0) {
                        GetClient().SendPacket(new DoorbellComposer(""));
                        room.SendPacket(new DoorbellComposer(GetClient().GetHabbo().Username), true);
                        return;
                    }

                    GetClient().SendPacket(new FlatAccessDeniedComposer(""));
                    GetClient().SendPacket(new CloseConnectionComposer());
                    return;
                }

                if (room.Access == RoomAccess.Password && !GetClient().GetHabbo().GetPermissions().HasRight("room_enter_locked")) {
                    if (password.ToLower() != room.Password.ToLower() || string.IsNullOrWhiteSpace(password)) {
                        GetClient().SendPacket(new GenericErrorComposer(-100002));
                        GetClient().SendPacket(new CloseConnectionComposer());
                        return;
                    }
                }
            }

            if (!EnterRoom(room)) {
                GetClient().SendPacket(new CloseConnectionComposer());
            }
        }

        public bool EnterRoom(Room room)
        {
            if (room == null) {
                GetClient().SendPacket(new CloseConnectionComposer());
                return false;
            }

            if (!RoomFactory.TryGetData(room.Id, out RoomData data)) {
                return false;
            }

            data.UsersNow++;
            GetClient().SendPacket(new RoomReadyComposer(room));
            if (room.Wallpaper != "0.0")
                GetClient().SendPacket(new RoomPropertyComposer("wallpaper", room.Wallpaper));
            if (room.Floor != "0.0")
                GetClient().SendPacket(new RoomPropertyComposer("floor", room.Floor));

            GetClient().SendPacket(new RoomPropertyComposer("landscape", room.Landscape));
            GetClient().SendPacket(new RoomRatingComposer(room.Score, !(GetClient().GetHabbo().RatedRooms.Contains(room.RoomId) || room.OwnerId == GetClient().GetHabbo().Id)));

            // The client only sends GetFurnitureAliases on the very first connection and then
            // stops. We must push FurnitureAliases proactively on every room entry so that the
            // client always receives it and triggers GetRoomEntryData in response.
            GetClient().SendPacket(new FurnitureAliasesComposer());
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                db.user_roomvisits.Add(new user_roomvisit
                {
                    user_id = (uint)GetClient().GetHabbo().Id,
                    room_id = (uint)GetClient().GetHabbo().CurrentRoomId,
                    entry_timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    exit_timestamp = 0,
                    hour = DateTime.Now.Hour,
                    minute = DateTime.Now.Minute
                });
                db.SaveChanges();
            }

            if (room.OwnerId != Id) {
                GetClient().GetHabbo().GetStats().RoomVisits += 1;
                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(GetClient(), "ACH_RoomEntry", 1);
            }
            return true;
        }
    }
}
