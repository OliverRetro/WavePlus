using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Core;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Commands;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
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
        public string Look { get; set; }
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
        public int TempInt { get; set; }
        public double LastHitTime { get; set; }
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

        public double CustomBuildHeight { get; set; }

        public Habbo(int id, string username, int rank, string motto, string look, string gender, int credits, int activityPoints, int homeRoom,
            bool hasFriendRequestsDisabled, int lastOnline, bool appearOffline, bool hideInRoom, double createDate, int diamonds,
            string machineId, string clientVolume, bool chatPreference, bool focusPreference, bool petsMuted, bool botsMuted, bool advertisingReportBlocked, double lastNameChange,
            int gotwPoints, bool ignoreInvites, double timeMuted, double tradingLock, bool allowGifts, int friendBarState, bool disableForcedEffects, bool allowMimic, int vipRank)
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
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.GetRowOrInsert(
                    "SELECT `user_id` FROM `user_info` WHERE `user_id` = @user_id LIMIT 1",
                    query => query.AddParameter("user_id", id),
                    "INSERT INTO `user_info` (`user_id`) VALUES (@user_id)",
                    query => query.AddParameter("user_id", id));

                if (TradingLockExpiry > 0 && PlusEnvironment.GetUnixTimestamp() > TradingLockExpiry)
                {
                    TradingLockExpiry = 0;
                    dbClient.SetQuery("UPDATE `user_info` SET `trading_locked` = 0 WHERE `user_id` = @user_id LIMIT 1");
                    dbClient.AddParameter("user_id", id);
                    dbClient.RunQuery();
                }
            }

            BannedPhraseCount = 0;
            SessionStart = PlusEnvironment.GetUnixTimestamp();
            MessengerSpamCount = 0;
            MessengerSpamTime = 0;
            CreditsUpdateTick = Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("user.currency_scheduler.tick"));

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
            CustomBubbleId = 0;
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

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DataRow statRow = dbClient.GetRowOrInsert(
                    "SELECT `id`,`roomvisits`,`onlinetime`,`respect`,`respectgiven`,`giftsgiven`,`giftsreceived`,`dailyrespectpoints`,`dailypetrespectpoints`,`achievementscore`,`quest_id`,`quest_progress`,`groupid`,`tickets_answered`,`respectstimestamp`,`forum_posts` FROM `user_stats` WHERE `id` = @user_id LIMIT 1",
                    query => query.AddParameter("user_id", id),
                    "INSERT INTO `user_stats` (`id`) VALUES (@user_id)",
                    query => query.AddParameter("user_id", id));

                try
                {
                    _habboStats = new HabboStats(
                        Convert.ToInt32(statRow["roomvisits"]),
                        Convert.ToDouble(statRow["onlineTime"]),
                        Convert.ToInt32(statRow["respect"]),
                        Convert.ToInt32(statRow["respectGiven"]),
                        Convert.ToInt32(statRow["giftsGiven"]),
                        Convert.ToInt32(statRow["giftsReceived"]),
                        Convert.ToInt32(statRow["dailyRespectPoints"]),
                        Convert.ToInt32(statRow["dailyPetRespectPoints"]),
                        Convert.ToInt32(statRow["AchievementScore"]),
                        Convert.ToInt32(statRow["quest_id"]),
                        Convert.ToInt32(statRow["quest_progress"]),
                        Convert.ToInt32(statRow["groupid"]),
                        Convert.ToString(statRow["respectsTimestamp"]),
                        Convert.ToInt32(statRow["forum_posts"])
                    );

                    if (Convert.ToString(statRow["respectsTimestamp"]) != DateTime.Today.ToString("MM/dd"))
                    {
                        _habboStats.RespectsTimestamp = DateTime.Today.ToString("MM/dd");

                        int dailyRespects = 10;

                        if (_permissions.HasRight("mod_tool"))
                            dailyRespects = 20;
                        else if (PlusEnvironment.GetGame().GetSubscriptionManager().TryGetSubscriptionData(vipRank, out SubscriptionData subData))
                            dailyRespects = subData.Respects;

                        _habboStats.DailyRespectPoints = dailyRespects;
                        _habboStats.DailyPetRespectPoints = dailyRespects;

                        dbClient.SetQuery("UPDATE `user_stats` SET `dailyRespectPoints` = @daily_respects, `dailyPetRespectPoints` = @daily_pet_respects, `respectsTimestamp` = @respects_timestamp WHERE `id` = @user_id LIMIT 1");
                        dbClient.AddParameter("daily_respects", dailyRespects);
                        dbClient.AddParameter("daily_pet_respects", dailyRespects);
                        dbClient.AddParameter("respects_timestamp", DateTime.Today.ToString("MM/dd"));
                        dbClient.AddParameter("user_id", id);
                        dbClient.RunQuery();
                    }
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                }
            }

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(_habboStats.FavouriteGroupId, out Group _))
                _habboStats.FavouriteGroupId = 0;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                DataRow statRow = dbClient.GetRowOrInsert(
                    "SELECT * FROM `user_rp_statistics` WHERE `user_id` = @user_id LIMIT 1",
                    query => query.AddParameter("user_id", id),
                    "INSERT INTO `user_rp_statistics` (`user_id`) VALUES (@user_id)",
                    query => query.AddParameter("user_id", id));

                try
                {
                    _rpStats = new UserRpStats(
                        id,
                        Convert.ToInt32(statRow["health"]),
                        Convert.ToInt32(statRow["shield"]),
                        Convert.ToInt32(statRow["energy"]),
                        Convert.ToInt32(statRow["hunger"]),
                        Convert.ToInt32(statRow["experience"]),
                        Convert.ToInt32(statRow["knockouts"]),
                        Convert.ToInt32(statRow["deaths"]),
                        Convert.ToInt32(statRow["arrested"]),
                        Convert.ToInt32(statRow["escapes"]),
                        Convert.ToInt32(statRow["damage_dealt"]),
                        Convert.ToInt32(statRow["damage_taken"]),
                        Convert.ToInt32(statRow["attacks"]),
                        Convert.ToInt32(statRow["attacked"]),
                        Convert.ToInt32(statRow["knowledge"]),
                        Convert.ToInt32(statRow["strength"]),
                        statRow.Table.Columns.Contains("is_dead") && Convert.ToInt32(statRow["is_dead"]) == 1,
                        statRow.Table.Columns.Contains("hospital_release_time") ? Convert.ToDouble(statRow["hospital_release_time"]) : 0,
                        statRow.Table.Columns.Contains("hospital_heal_start_time") ? Convert.ToDouble(statRow["hospital_heal_start_time"]) : 0,
                        statRow.Table.Columns.Contains("hospital_health_start") ? Convert.ToInt32(statRow["hospital_health_start"]) : 0
                    );
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                }
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`weapon_id`,`durability_left` FROM `user_rp_weapons` WHERE `user_id` = @user_id");
                dbClient.AddParameter("user_id", id);
                DataTable weaponTable = dbClient.GetTable();

                List<UserWeapon> loadedWeapons = new();
                if (weaponTable != null)
                {
                    foreach (DataRow row in weaponTable.Rows)
                    {
                        int weaponId = Convert.ToInt32(row["weapon_id"]);
                        Weapon weaponData = PlusEnvironment.GetWeaponManager().GetWeaponById(weaponId);
                        if (weaponData == null)
                            continue;

                        loadedWeapons.Add(new UserWeapon(Convert.ToInt32(row["id"]), id, weaponId, Convert.ToInt32(row["durability_left"]), weaponData));
                    }
                }

                _rpWeapons = new UserRpWeapons(id, PlusEnvironment.GetWeaponManager(), loadedWeapons);
                _rpWeapons.MarkSaved();
                _rpStats.Weapons = _rpWeapons;
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`skin_id`,`equipped` FROM `user_rp_weapon_skins` WHERE `user_id` = @user_id");
                dbClient.AddParameter("user_id", id);
                DataTable skinTable = dbClient.GetTable();

                List<UserWeaponSkin> loadedSkins = new();
                if (skinTable != null)
                {
                    foreach (DataRow row in skinTable.Rows)
                    {
                        WeaponSkin skinData = PlusEnvironment.GetWeaponManager().GetSkinById(Convert.ToInt32(row["skin_id"]));
                        if (skinData == null)
                            continue;

                        loadedSkins.Add(new UserWeaponSkin(Convert.ToInt32(row["id"]), id, skinData.Id, Convert.ToInt32(row["equipped"]) == 1, skinData));
                    }
                }

                _rpWeaponSkins = new UserRpWeaponSkins(loadedSkins);
                _rpWeaponSkins.MarkSaved();
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `skill_id`,`progress`,`equipped` FROM `user_rp_skills` WHERE `user_id` = @user_id");
                dbClient.AddParameter("user_id", id);
                DataTable skillTable = dbClient.GetTable();

                List<UserSkillData> loadedSkills = new();
                if (skillTable != null)
                {
                    foreach (DataRow row in skillTable.Rows)
                    {
                        loadedSkills.Add(new UserSkillData(
                            Convert.ToInt32(row["skill_id"]),
                            Convert.ToInt32(row["progress"]),
                            Convert.ToInt32(row["equipped"]) == 1));
                    }
                }

                _rpSkills = new UserRpSkills(PlusEnvironment.GetSkillManager(), loadedSkills);
                _rpSkills.MarkSaved();
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
            return _rpStats;
        }

        public int GetReconnectRoomId()
        {
            return HomeRoomData?.roomid > 0 ? HomeRoomData.roomid : HomeRoom;
        }

        public void LoadHomeRoomData(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
            {
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

        public void ApplyHomeRoomData()
        {
            if (!PendingReconnectPlacement || HomeRoomData == null || CurrentRoom == null || CurrentRoomId != HomeRoomData.roomid)
                return;

            RoomUser roomUser = CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Id);
            if (roomUser == null)
                return;

            int targetX = HomeRoomData.x;
            int targetY = HomeRoomData.y;
            if (!CurrentRoom.GetGameMap().ValidTile(targetX, targetY) || !CurrentRoom.GetGameMap().SquareIsOpen(targetX, targetY, false))
            {
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

            if (GetRpWeapons() != null && GetRpWeapons().SetActiveWeaponId(HomeRoomData.weapon))
            {
                UserWeapon activeWeapon = GetRpWeapons().GetActiveWeapon();
                UserWeaponSkin equippedSkin = GetRpWeaponSkins()?.GetEquippedSkinForWeapon(activeWeapon.WeaponId);
                Effects().ApplyEffect(equippedSkin?.SkinData?.EffectId ?? activeWeapon.WeaponData.Effect);
            }

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

            if (isEquipped)
            {
                if (_rpStats.Health > 100)
                {
                    CachedSkillHealth = _rpStats.Health;
                    HasCachedSkillHealth = true;
                    _rpStats.Health = 100;
                }
            }
            else if (HasCachedSkillHealth)
            {
                if (CachedSkillHealth > _rpStats.Health)
                    _rpStats.Health = CachedSkillHealth;

                CachedSkillHealth = 0;
                HasCachedSkillHealth = false;
            }

            return true;
        }

        public void SaveRpStats()
        {
            try
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.UpdateOrInsert(
                        "UPDATE `user_rp_statistics` SET `health` = @health, `shield` = @shield, `energy` = @energy, `hunger` = @hunger, `experience` = @experience, `knockouts` = @knockouts, `deaths` = @deaths, `arrested` = @arrested, `escapes` = @escapes, `damage_dealt` = @damage_dealt, `damage_taken` = @damage_taken, `attacks` = @attacks, `attacked` = @attacked, `knowledge` = @knowledge, `strength` = @strength, `is_dead` = @is_dead, `hospital_release_time` = @hospital_release_time, `hospital_heal_start_time` = @hospital_heal_start_time, `hospital_health_start` = @hospital_health_start WHERE `user_id` = @user_id LIMIT 1",
                        query =>
                        {
                            query.AddParameter("health", _rpStats.Health);
                            query.AddParameter("shield", _rpStats.Shield);
                            query.AddParameter("energy", _rpStats.Energy);
                            query.AddParameter("hunger", _rpStats.Hunger);
                            query.AddParameter("experience", _rpStats.Experience);
                            query.AddParameter("knockouts", _rpStats.Knockouts);
                            query.AddParameter("deaths", _rpStats.Deaths);
                            query.AddParameter("arrested", _rpStats.Arrested);
                            query.AddParameter("escapes", _rpStats.Escapes);
                            query.AddParameter("damage_dealt", _rpStats.DamageDealt);
                            query.AddParameter("damage_taken", _rpStats.DamageTaken);
                            query.AddParameter("attacks", _rpStats.Attacks);
                            query.AddParameter("attacked", _rpStats.Attacked);
                            query.AddParameter("knowledge", _rpStats.Knowledge);
                            query.AddParameter("strength", _rpStats.Strength);
                            query.AddParameter("is_dead", _rpStats.IsDead ? 1 : 0);
                            query.AddParameter("hospital_release_time", _rpStats.HospitalReleaseTime);
                            query.AddParameter("hospital_heal_start_time", _rpStats.HospitalHealStartTime);
                            query.AddParameter("hospital_health_start", _rpStats.HospitalHealthStart);
                            query.AddParameter("user_id", Id);
                        },
                        "INSERT INTO `user_rp_statistics` (`user_id`) VALUES (@user_id)",
                        query =>
                        {
                            query.AddParameter("user_id", Id);
                        });
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        public void SaveRpSkills()
        {
            if (_rpSkills == null || !_rpSkills.Dirty)
                return;

            try
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    foreach (UserSkillData skill in _rpSkills.Skills)
                    {
                        dbClient.UpdateOrInsert(
                            "UPDATE `user_rp_skills` SET `progress` = @progress, `equipped` = @equipped WHERE `user_id` = @user_id AND `skill_id` = @skill_id LIMIT 1",
                            query =>
                            {
                                query.AddParameter("progress", skill.Progress);
                                query.AddParameter("equipped", skill.Equipped ? 1 : 0);
                                query.AddParameter("user_id", Id);
                                query.AddParameter("skill_id", skill.SkillId);
                            },
                            "INSERT INTO `user_rp_skills` (`user_id`, `skill_id`, `progress`, `equipped`) VALUES (@user_id, @skill_id, @progress, @equipped)",
                            query =>
                            {
                                query.AddParameter("user_id", Id);
                                query.AddParameter("skill_id", skill.SkillId);
                                query.AddParameter("progress", skill.Progress);
                                query.AddParameter("equipped", skill.Equipped ? 1 : 0);
                            });
                    }
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
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    foreach (UserWeapon weapon in _rpWeapons.Weapons)
                    {
                        if (weapon.IsDefault)
                            continue;

                        if (weapon.Id > 0)
                        {
                            dbClient.SetQuery("UPDATE `user_rp_weapons` SET `durability_left` = @durability_left WHERE `id` = @id LIMIT 1");
                            dbClient.AddParameter("durability_left", weapon.DurabilityLeft);
                            dbClient.AddParameter("id", weapon.Id);
                            dbClient.RunQuery();
                            continue;
                        }

                        dbClient.SetQuery("INSERT INTO `user_rp_weapons` (`user_id`, `weapon_id`, `durability_left`) VALUES (@user_id, @weapon_id, @durability_left)");
                        dbClient.AddParameter("user_id", Id);
                        dbClient.AddParameter("weapon_id", weapon.WeaponId);
                        dbClient.AddParameter("durability_left", weapon.DurabilityLeft);
                        int oldId = weapon.Id;
                        weapon.Id = (int)dbClient.InsertQuery();
                        _rpWeapons.RekeyWeapon(oldId, weapon.Id);
                    }
                }

                _rpWeapons.MarkSaved();
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
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    foreach (UserWeaponSkin skin in _rpWeaponSkins.Skins)
                    {
                        if (skin.Id > 0)
                        {
                            dbClient.SetQuery("UPDATE `user_rp_weapon_skins` SET `equipped` = @equipped WHERE `id` = @id LIMIT 1");
                            dbClient.AddParameter("equipped", skin.Equipped ? 1 : 0);
                            dbClient.AddParameter("id", skin.Id);
                            dbClient.RunQuery();
                            continue;
                        }

                        dbClient.SetQuery("INSERT INTO `user_rp_weapon_skins` (`user_id`, `skin_id`, `equipped`) VALUES (@user_id, @skin_id, @equipped)");
                        dbClient.AddParameter("user_id", Id);
                        dbClient.AddParameter("skin_id", skin.SkinId);
                        dbClient.AddParameter("equipped", skin.Equipped ? 1 : 0);
                        int oldId = skin.Id;
                        skin.Id = (int)dbClient.InsertQuery();
                        _rpWeaponSkins.RekeySkin(oldId, skin.Id);
                    }
                }

                _rpWeaponSkins.MarkSaved();
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
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
                _habboSaved = true;
                return "UPDATE `users` SET `online` = '0', `last_online` = '" + PlusEnvironment.GetUnixTimestamp() + "', `activity_points` = '" + Duckets + "', `credits` = '" + Credits + "', `vip_points` = '" + Diamonds + "', `home_room` = '" + HomeRoom + "', `gotw_points` = '" + GotwPoints + "', `time_muted` = '" + TimeMuted + "',`friend_bar_state` = '" + FriendBarStateUtility.GetInt(FriendBarState) + "' WHERE id = '" + Id + "' LIMIT 1;UPDATE `user_stats` SET `roomvisits` = '" + _habboStats.RoomVisits + "', `onlineTime` = '" + (PlusEnvironment.GetUnixTimestamp() - SessionStart + _habboStats.OnlineTime) + "', `respect` = '" + _habboStats.Respect + "', `respectGiven` = '" + _habboStats.RespectGiven + "', `giftsGiven` = '" + _habboStats.GiftsGiven + "', `giftsReceived` = '" + _habboStats.GiftsReceived + "', `dailyRespectPoints` = '" + _habboStats.DailyRespectPoints + "', `dailyPetRespectPoints` = '" + _habboStats.DailyPetRespectPoints + "', `AchievementScore` = '" + _habboStats.AchievementPoints + "', `quest_id` = '" + _habboStats.QuestId + "', `quest_progress` = '" + _habboStats.QuestProgress + "', `groupid` = '" + _habboStats.FavouriteGroupId + "',`forum_posts` = '" + _habboStats.ForumPosts + "' WHERE `id` = '" + Id + "' LIMIT 1;";
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

            if (!_habboSaved)
            {
                _habboSaved = true;
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE `users` SET `online` = '0', `last_online` = @last_online, `activity_points` = @activity_points, `credits` = @credits, `vip_points` = @vip_points, `home_room` = @home_room, `home_room_data` = @home_room_data, `gotw_points` = @gotw_points, `time_muted` = @time_muted, `friend_bar_state` = @friend_bar_state WHERE id = @id LIMIT 1;");
                    dbClient.AddParameter("last_online", PlusEnvironment.GetUnixTimestamp());
                    dbClient.AddParameter("activity_points", Duckets);
                    dbClient.AddParameter("credits", Credits);
                    dbClient.AddParameter("vip_points", Diamonds);
                    dbClient.AddParameter("home_room", HomeRoom);
                    dbClient.AddParameter("home_room_data", SerializeHomeRoomData());
                    dbClient.AddParameter("gotw_points", GotwPoints);
                    dbClient.AddParameter("time_muted", TimeMuted);
                    dbClient.AddParameter("friend_bar_state", FriendBarStateUtility.GetInt(FriendBarState));
                    dbClient.AddParameter("id", Id);
                    dbClient.RunQuery();

                    dbClient.RunQuery("UPDATE `user_stats` SET `roomvisits` = '" + _habboStats.RoomVisits + "', `onlineTime` = '" + (PlusEnvironment.GetUnixTimestamp() - SessionStart + _habboStats.OnlineTime) + "', `respect` = '" + _habboStats.Respect + "', `respectGiven` = '" + _habboStats.RespectGiven + "', `giftsGiven` = '" + _habboStats.GiftsGiven + "', `giftsReceived` = '" + _habboStats.GiftsReceived + "', `dailyRespectPoints` = '" + _habboStats.DailyRespectPoints + "', `dailyPetRespectPoints` = '" + _habboStats.DailyPetRespectPoints + "', `AchievementScore` = '" + _habboStats.AchievementPoints + "', `quest_id` = '" + _habboStats.QuestId + "', `quest_progress` = '" + _habboStats.QuestProgress + "', `groupid` = '" + _habboStats.FavouriteGroupId + "',`forum_posts` = '" + _habboStats.ForumPosts + "' WHERE `id` = '" + Id + "' LIMIT 1;");

                    if (GetPermissions().HasRight("mod_tickets"))
                        dbClient.RunQuery("UPDATE `moderation_tickets` SET `status` = 'open', `moderator_id` = '0' WHERE `status` ='picked' AND `moderator_id` = '" + Id + "'");
                }

                SaveRpStats();
                SaveRpWeapons();
                SaveRpWeaponSkins();
                SaveRpSkills();
            }

            Dispose();

            _client = null;
        }

        public void Dispose()
        {
            _inventoryComponent?.SetIdleState();

            if (InRoom && CurrentRoom != null)
                CurrentRoom.GetRoomUserManager().RemoveUserFromRoom(_client, false);

            if (_messenger != null)
            {
                _messenger.AppearOffline = true;
                _messenger.Destroy();
            }

            _fx?.Dispose();

            _clothing?.Dispose();

            _permissions?.Dispose();

            if (_ignores != null)
                _permissions.Dispose();
        }

        public void CheckCreditsTimer()
        {
            try
            {
                CreditsUpdateTick--;

                if (CreditsUpdateTick <= 0)
                {
                    int creditUpdate = Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("user.currency_scheduler.credit_reward"));
                    int ducketUpdate = Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("user.currency_scheduler.ducket_reward"));

                    if (PlusEnvironment.GetGame().GetSubscriptionManager().TryGetSubscriptionData(VipRank, out SubscriptionData subData))
                    {
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
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET " + key + " = @value WHERE `id` = '" + Id + "' LIMIT 1;");
                dbClient.AddParameter("value", value);
                dbClient.RunQuery();
            }
        }

        public void PrepareRoom(int id, string password)
        {
            if (GetClient() == null || GetClient().GetHabbo() == null)
                return;

            if (GetClient().GetHabbo().InRoom)
            {
                if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(GetClient().GetHabbo().CurrentRoomId, out Room oldRoom))
                    return;

                if (oldRoom.GetRoomUserManager() != null)
                    oldRoom.GetRoomUserManager().RemoveUserFromRoom(GetClient(), false);
            }

            if (GetClient().GetHabbo().IsTeleporting && GetClient().GetHabbo().TeleportingRoomId != id)
            {
                GetClient().SendPacket(new CloseConnectionComposer());
                return;
            }

            if (!PlusEnvironment.GetGame().GetRoomManager().TryLoadRoom(id, out Room room))
            {
                GetClient().SendPacket(new CloseConnectionComposer());
                return;
            }

            if (room.IsCrashed)
            {
                GetClient().SendNotification("This room has crashed! :(");
                GetClient().SendPacket(new CloseConnectionComposer());
                return;
            }

            GetClient().GetHabbo().CurrentRoomId = room.RoomId;

            if (room.GetRoomUserManager().UserCount >= room.UsersMax && !GetClient().GetHabbo().GetPermissions().HasRight("room_enter_full") && GetClient().GetHabbo().Id != room.OwnerId)
            {
                GetClient().SendPacket(new CantConnectComposer(1));
                GetClient().SendPacket(new CloseConnectionComposer());
                return;
            }


            if (!GetPermissions().HasRight("room_ban_override") && room.GetBans().IsBanned(Id))
            {
                RoomAuthOk = false;
                GetClient().GetHabbo().RoomAuthOk = false;
                GetClient().SendPacket(new CantConnectComposer(4));
                GetClient().SendPacket(new CloseConnectionComposer());
                return;
            }

            GetClient().SendPacket(new OpenConnectionComposer());
            if (!room.CheckRights(GetClient(), true, true) && !GetClient().GetHabbo().IsTeleporting && !GetClient().GetHabbo().IsHopping)
            {
                if (room.Access == RoomAccess.Doorbell && !GetClient().GetHabbo().GetPermissions().HasRight("room_enter_locked"))
                {
                    if (room.UserCount > 0)
                    {
                        GetClient().SendPacket(new DoorbellComposer(""));
                        room.SendPacket(new DoorbellComposer(GetClient().GetHabbo().Username), true);
                        return;
                    }

                    GetClient().SendPacket(new FlatAccessDeniedComposer(""));
                    GetClient().SendPacket(new CloseConnectionComposer());
                    return;
                }

                if (room.Access == RoomAccess.Password && !GetClient().GetHabbo().GetPermissions().HasRight("room_enter_locked"))
                {
                    if (password.ToLower() != room.Password.ToLower() || string.IsNullOrWhiteSpace(password))
                    {
                        GetClient().SendPacket(new GenericErrorComposer(-100002));
                        GetClient().SendPacket(new CloseConnectionComposer());
                        return;
                    }
                }
            }

            if (!EnterRoom(room))
            {
                GetClient().SendPacket(new CloseConnectionComposer());
            }
        }

        public bool EnterRoom(Room room)
        {
            if (room == null)
            {
                GetClient().SendPacket(new CloseConnectionComposer());
                return false;
            }

            if (!RoomFactory.TryGetData(room.Id, out RoomData data))
            {
                return false;
            }

            data.UsersNow++;
            GetClient().SendPacket(new RoomReadyComposer(room.Id, room.ModelName));
            if (room.Wallpaper != "0.0")
                GetClient().SendPacket(new RoomPropertyComposer("wallpaper", room.Wallpaper));
            if (room.Floor != "0.0")
                GetClient().SendPacket(new RoomPropertyComposer("floor", room.Floor));

            GetClient().SendPacket(new RoomPropertyComposer("landscape", room.Landscape));
            GetClient().SendPacket(new RoomRatingComposer(room.Score, !(GetClient().GetHabbo().RatedRooms.Contains(room.RoomId) || room.OwnerId == GetClient().GetHabbo().Id)));
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("INSERT INTO user_roomvisits (user_id,room_id,entry_timestamp,exit_timestamp,hour,minute) VALUES ('" + GetClient().GetHabbo().Id + "','" + GetClient().GetHabbo().CurrentRoomId + "','" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + "','0','" + DateTime.Now.Hour + "','" + DateTime.Now.Minute + "');"); // +
            }

            if (room.OwnerId != Id)
            {
                GetClient().GetHabbo().GetStats().RoomVisits += 1;
                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(GetClient(), "ACH_RoomEntry", 1);
            }
            return true;
        }
    }
}
