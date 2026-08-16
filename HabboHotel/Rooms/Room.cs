using DotNetty.Buffers;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Core;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Data.Moodlight;
using Plus.HabboHotel.Items.Data.Toner;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.AI.Speech;
using Plus.HabboHotel.Rooms.Games;
using Plus.HabboHotel.Rooms.Games.Banzai;
using Plus.HabboHotel.Rooms.Games.Football;
using Plus.HabboHotel.Rooms.Games.Freeze;
using Plus.HabboHotel.Rooms.Games.Teams;
using Plus.HabboHotel.Rooms.Instance;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Rooms
{
    public class Room : RoomData
    {
        public bool IsCrashed;
        public bool mDisposed;
        public bool RoomMuted;
        public DateTime LastTimerReset;
        public DateTime LastRegeneration;

        public Task ProcessTask;
        public ArrayList ActiveTrades;

        public TonerData TonerData;
        public MoodlightData MoodlightData;

        public Dictionary<int, double> MutedUsers;

        private readonly Dictionary<int, List<RoomUser>> _tents;

        public List<int> UsersWithRights;
        private GameManager _gameManager;
        private Freeze _freeze;
        private Soccer _soccer;
        private BattleBanzai _banzai;

        private Gamemap _gamemap;
        private GameItemHandler _gameItemHandler;

        public TeamManager TeamBanzai;
        public TeamManager TeamFreeze;

        private RoomUserManager _roomUserManager;
        private RoomItemHandling _roomItemHandling;

        private readonly FilterComponent _filterComponent;
        private readonly WiredComponent _wiredComponent;
        private readonly BansComponent _bansComponent;
        private readonly TradingComponent _tradingComponent;

        public int IsLagging { get; set; }
        public bool Unloaded { get; set; }
        public int IdleTime { get; set; }

        public int LastPersistedUsersNow = -1;

        // Timestamp (unix ms) of the last full 500ms cycle. ProcessRoom now runs every 50ms
        // for responsive movement; everything else (items, wired, timers) stays on 500ms.
        private long _lastFullCycleMs;

        public Room(RoomData data)
            : base(data)
        {
            IsLagging = 0;
            Unloaded = false;
            IdleTime = 0;

            RoomMuted = false;

            MutedUsers = [];
            _tents = [];

            _gamemap = new Gamemap(this, data.Model);
            _roomItemHandling = new RoomItemHandling(this);

            _roomUserManager = new RoomUserManager(this);
            _filterComponent = new FilterComponent(this);
            _wiredComponent = new WiredComponent(this);
            _bansComponent = new BansComponent(this);
            _tradingComponent = new TradingComponent(this);
            ActiveTrades = [];

            GetRoomItemHandler().LoadFurniture();
            GetGameMap().GenerateMaps();

            LoadPromotions();
            LoadRights();
            LoadFilter();
            InitBots();
            InitPets();

            LastRegeneration = DateTime.Now;
        }

        public List<string> WordFilterList { get; set; }

        public int UserCount
        {
            get
            {
                // Allocation-free live head-count (the old version built a new list per call).
                RoomUserManager manager = _roomUserManager;
                if (manager == null)
                    return 0;

                int count = 0;
                foreach (RoomUser user in manager.GetUserList()) {
                    if (user != null && !user.IsBot)
                        count++;
                }

                return count;
            }
        }

        public int RoomId => Id;

        public Gamemap GetGameMap()
        {
            return _gamemap;
        }

        public RoomItemHandling GetRoomItemHandler()
        {
            _roomItemHandling ??= new RoomItemHandling(this);

            return _roomItemHandling;
        }

        public RoomUserManager GetRoomUserManager()
        {
            return _roomUserManager;
        }

        public Soccer GetSoccer()
        {
            _soccer ??= new Soccer(this);

            return _soccer;
        }

        public TeamManager GetTeamManagerForBanzai()
        {
            TeamBanzai ??= TeamManager.CreateTeam("banzai");
            return TeamBanzai;
        }

        public TeamManager GetTeamManagerForFreeze()
        {
            TeamFreeze ??= TeamManager.CreateTeam("freeze");
            return TeamFreeze;
        }

        public BattleBanzai GetBanzai()
        {
            _banzai ??= new BattleBanzai(this);
            return _banzai;
        }

        public Freeze GetFreeze()
        {
            _freeze ??= new Freeze(this);
            return _freeze;
        }

        public GameManager GetGameManager()
        {
            _gameManager ??= new GameManager(this);
            return _gameManager;
        }

        public GameItemHandler GetGameItemHandler()
        {
            _gameItemHandler ??= new GameItemHandler(this);
            return _gameItemHandler;
        }

        public bool GotSoccer()
        {
            return _soccer != null;
        }

        public bool GotBanzai()
        {
            return _banzai != null;
        }

        public bool GotFreeze()
        {
            return _freeze != null;
        }

        public void ClearTags()
        {
            Tags.Clear();
        }

        public void AddTagRange(List<string> tags)
        {
            Tags.AddRange(tags);
        }

        public void InitBots()
        {
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            uint roomKey = (uint)RoomId;
            var bots = db.Bots.Where(b => b.RoomId == roomKey && b.AiType != "pet").ToList();

            if (bots.Count == 0)
                return;

            // One speech query for the whole room instead of one per bot (N+1 on room load).
            var botIds = bots.Select(b => b.Id).ToList();
            var speechesByBot = db.BotsSpeeches.Where(s => botIds.Contains(s.BotId))
                .Select(s => new { s.BotId, s.Text }).ToList()
                .GroupBy(s => s.BotId)
                .ToDictionary(g => g.Key, g => g.Select(s => s.Text).ToList());

            foreach (var bot in bots) {
                List<RandomSpeech> speeches = speechesByBot.TryGetValue(bot.Id, out var texts)
                    ? [.. texts.Select(t => new RandomSpeech(t, (int)bot.Id))]
                    : [];

                _roomUserManager.DeployBot(new RoomBot((int)bot.Id, (int)bot.RoomId, bot.AiType, bot.WalkMode, bot.Name, bot.Motto, bot.Look, bot.X, bot.Y, bot.Z, bot.Rotation, 0, 0, 0, 0, ref speeches, bot.Gender, 0, (int)bot.UserId, Convert.ToBoolean(bot.AutomaticChat), bot.SpeakingInterval, PlusEnvironment.EnumToBool(bot.MixSentences), bot.ChatBubble, bot.EffectId), null);
            }
        }

        public void InitPets()
        {
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            uint roomKey = (uint)RoomId;
            var pets = db.Bots.Where(b => b.RoomId == roomKey && b.AiType == "pet")
                .Select(b => new { b.Id, b.UserId, b.RoomId, b.Name, b.X, b.Y, b.Z }).ToList();

            if (pets.Count == 0)
                return;

            // One pet-data query for the whole room instead of one per pet (N+1 on room load).
            var petIds = pets.Select(p => p.Id).ToList();
            var petData = db.BotsPetdata.Where(p => petIds.Contains(p.Id))
                .Select(p => new { p.Id, p.Type, p.Race, p.Color, p.Experience, p.Energy, p.Nutrition, p.Respect, p.Createstamp, p.HaveSaddle, p.AnyoneRide, p.Hairdye, p.Pethair, p.GnomeClothing })
                .ToList()
                .ToDictionary(p => p.Id);

            foreach (var row in pets) {
                if (!petData.TryGetValue(row.Id, out var mRow))
                    continue;

                Pet pet = new((int)row.Id, (int)row.UserId, (int)row.RoomId, row.Name, (int)(mRow.Type ?? 0), mRow.Race,
                    mRow.Color, mRow.Experience ?? 0, mRow.Energy ?? 0, mRow.Nutrition ?? 0, mRow.Respect ?? 0, (double)(mRow.Createstamp ?? 0), row.X, row.Y,
                    (double)row.Z, mRow.HaveSaddle ?? 0, mRow.AnyoneRide ?? 0, mRow.Hairdye ?? 0, mRow.Pethair ?? 0, mRow.GnomeClothing);

                var rndSpeechList = new List<RandomSpeech>();

                _roomUserManager.DeployBot(new RoomBot(pet.PetId, RoomId, "pet", "freeroam", pet.Name, "", pet.Look, pet.X, pet.Y, Convert.ToInt32(pet.Z), 0, 0, 0, 0, 0, ref rndSpeechList, "", 0, pet.OwnerId, false, 0, false, 0, 0), pet);
            }
        }

        public FilterComponent GetFilter()
        {
            return _filterComponent;
        }

        public WiredComponent GetWired()
        {
            return _wiredComponent;
        }

        public BansComponent GetBans()
        {
            return _bansComponent;
        }

        public TradingComponent GetTrading()
        {
            return _tradingComponent;
        }

        public void LoadRights()
        {
            UsersWithRights = [];
            if (Group != null)
                return;

            uint roomKey = (uint)Id;
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            UsersWithRights = [.. db.RoomRights.Where(r => r.RoomId == roomKey).Select(r => (int)r.UserId)];
        }

        private void LoadFilter()
        {
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            WordFilterList = [.. db.RoomFilters.Where(f => f.RoomId == Id).Select(f => f.Word)];
        }

        public bool CheckRights(GameClient session)
        {
            return CheckRights(session, false);
        }

        public bool CheckRights(GameClient session, bool requireOwnership, bool checkForGroups = false)
        {
            try {
                if (session == null || session.GetHabbo() == null)
                    return false;

                if (session.GetHabbo().Username == OwnerName && Type == "private")
                    return true;

                if (session.GetHabbo().GetPermissions().HasRight("room_any_owner"))
                    return true;

                if (!requireOwnership && Type == "private") {
                    if (session.GetHabbo().GetPermissions().HasRight("room_any_rights"))
                        return true;

                    if (UsersWithRights.Contains(session.GetHabbo().Id))
                        return true;
                }

                if (checkForGroups && Type == "private") {
                    if (Group == null)
                        return false;

                    if (Group.IsAdmin(session.GetHabbo().Id))
                        return true;

                    if (Groups.GroupManager.IsGangKind(Group.Kind) && Group.IsMember(session.GetHabbo().Id))
                        return true;

                    if (Group.AdminOnlyDeco == 0) {
                        if (Group.IsAdmin(session.GetHabbo().Id))
                            return true;
                    }
                }
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }

            return false;
        }

        public void OnUserShoot(RoomUser user, Item ball)
        {
            Func<Item, bool> predicate = null;
            string key = null;
            foreach (Item item in GetRoomItemHandler().GetFurniObjects(ball.GetX, ball.GetY).ToList()) {
                if (item.GetBaseItem().ItemName.StartsWith("fball_goal_")) {
                    key = item.GetBaseItem().ItemName.Split(['_'])[2];
                    user.UnIdle();
                    user.DanceId = 0;

                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(user.GetClient(), "ACH_FootballGoalScored", 1);

                    SendPacket(new ActionComposer(user.VirtualId, 1));
                }
            }

            if (key != null) {
                predicate ??= p => p.GetBaseItem().ItemName == "fball_score_" + key;

                foreach (Item item2 in GetRoomItemHandler().GetFloor.Where(predicate).ToList()) {
                    if (item2.GetBaseItem().ItemName == "fball_score_" + key) {
                        if (!string.IsNullOrEmpty(item2.ExtraData))
                            item2.ExtraData = (Convert.ToInt32(item2.ExtraData) + 1).ToString();
                        else
                            item2.ExtraData = "1";
                        item2.UpdateState();
                    }
                }
            }
        }

        public void ProcessRoom()
        {
            if (IsCrashed || mDisposed)
                return;

            try {
                long nowMs = PlusEnvironment.Now();
                bool fullCycle = nowMs - _lastFullCycleMs >= 500;
                if (fullCycle)
                    _lastFullCycleMs = nowMs;

                if (fullCycle) {
                    if (GetRoomUserManager().UserCount == 0)
                        IdleTime++;
                    else if (IdleTime > 0)
                        IdleTime = 0;

                    if (HasActivePromotion && Promotion.HasExpired) {
                        EndPromotion();
                    }

                    if (IdleTime >= 60 && !HasActivePromotion && !IsPublic) {
                        PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(Id);
                        return;
                    }

                    try {
                        GetRoomItemHandler().OnCycle();
                    } catch (Exception e) {
                        ExceptionLogger.LogException(e);
                    }
                }

                try {
                    GetRoomUserManager().OnCycle(fullCycle);
                } catch (Exception e) {
                    ExceptionLogger.LogException(e);
                }

                if (fullCycle) {
                    try {
                        Roleplay.Utilities.FurnitureAchievementService.Tick(this);
                    } catch (Exception e) {
                        ExceptionLogger.LogException(e);
                    }
                }

                #region Status Updates

                try {
                    // Runs on every 50ms tick so freshly-issued steps reach clients immediately.
                    GetRoomUserManager().SerializeStatusUpdates();
                } catch (Exception e) {
                    ExceptionLogger.LogException(e);
                }

                #endregion

                if (fullCycle) {
                    #region Game Item Cycle

                    try {
                        _gameItemHandler?.OnCycle();
                    } catch (Exception e) {
                        ExceptionLogger.LogException(e);
                    }

                    #endregion

                    try {
                        GetWired().OnCycle();
                    } catch (Exception e) {
                        ExceptionLogger.LogException(e);
                    }
                }
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
                OnRoomCrash(e);
            }
        }

        private void OnRoomCrash(Exception e)
        {
            try {
                foreach (RoomUser user in _roomUserManager.GetRoomUsers().ToList()) {
                    if (user == null || user.GetClient() == null)
                        continue;

                    user.GetClient().SendNotification("Sorry, it appears that room has crashed!"); //Unhandled exception in room: " + e);

                    try {
                        GetRoomUserManager().RemoveUserFromRoom(user.GetClient(), true);
                    } catch (Exception e2) {
                        ExceptionLogger.LogException(e2);
                    }
                }
            } catch (Exception e3) {
                ExceptionLogger.LogException(e3);
            }

            IsCrashed = true;
            PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(Id);
        }

        public bool CheckMute(GameClient session)
        {
            if (MutedUsers.ContainsKey(session.GetHabbo().Id)) {
                if (MutedUsers[session.GetHabbo().Id] < PlusEnvironment.GetUnixTimestamp()) {
                    MutedUsers.Remove(session.GetHabbo().Id);
                } else {
                    return true;
                }
            }

            if (session.GetHabbo().TimeMuted > 0 || RoomMuted && session.GetHabbo().Username != OwnerName)
                return true;

            return false;
        }

        public void SendObjects(GameClient session, Room room)
        {
            List<RoomUser> visibleUsers = [.. _roomUserManager.GetUserList().Where(u => u != null)];

            ServerPacket enterRoomPacketsMerged = new ServerPacket(-1, Unpooled.Buffer());
            new HeightMapComposer(room).Compose(enterRoomPacketsMerged);
            new FloorHeightMapComposer(GetGameMap().Model.GetRelativeHeightmap(), GetGameMap().StaticModel.WallHeight).Compose(enterRoomPacketsMerged);

            new UsersComposer(visibleUsers).Compose(enterRoomPacketsMerged);
            new ObjectsComposer([.. GetRoomItemHandler().GetFloor], this).Compose(enterRoomPacketsMerged);
            new ItemsComposer([.. GetRoomItemHandler().GetWall], this).Compose(enterRoomPacketsMerged);

            new RoomEntryInfoComposer(room.RoomId, room.CheckRights(session, true)).Compose(enterRoomPacketsMerged);
            new RoomVisualizationSettingsComposer(room.WallThickness, room.FloorThickness, PlusEnvironment.EnumToBool(room.HideWall.ToString())).Compose(enterRoomPacketsMerged);
            new RoomReadyComposer(room).Compose(enterRoomPacketsMerged);

            RoomFactory.TryGetData(room.Id, out RoomData data);

            List<MessageComposer> entry = [
                new RoomInitDataComposer(enterRoomPacketsMerged),
                new UsersComposer(visibleUsers),
                new UserUpdateComposer(visibleUsers),
                new GetGuestRoomResultComposer(session, data, true, false)
            ];

            foreach (RoomUser user in visibleUsers) {
                if (user == null)
                    continue;

                if (user.IsBot && user.BotData.DanceId > 0)
                    entry.Add(new DanceComposer(user.VirtualId, user.BotData.DanceId));
                else if (!user.IsBot && !user.IsPet && user.IsDancing)
                    entry.Add(new DanceComposer(user.VirtualId, user.DanceId));

                if (user.IsBot && user.BotData.EffectId > 0)
                    entry.Add(new AvatarEffectComposer(user.VirtualId, user.BotData.EffectId));

                if (user.IsAsleep)
                    entry.Add(new SleepComposer(user.VirtualId, true));

                if (user.CarryItemId > 0 && user.CarryTimer > 0)
                    entry.Add(new CarryObjectComposer(user.VirtualId, user.CarryItemId));

                if (!user.IsBot && !user.IsPet && user.CurrentEffect > 0)
                    entry.Add(new AvatarEffectComposer(user.VirtualId, user.CurrentEffect));

                // Catch the arriving client up on anyone already walking at a non-default tier;
                // the client assumes 1.0 for every avatar it has not been told about.
                if (user.WalkSpeed != RoomUser.WalkSpeedNormal)
                    entry.Add(new Plus.Communication.Packets.Outgoing.Roleplay.AvatarWalkSpeedComposer(user.VirtualId, user.WalkSpeed, user.StepAnimationMs));
            }

            session.SendPackets(entry);
        }

        #region Tents

        public void AddTent(int tentId)
        {
            _tents.Remove(tentId);

            _tents.Add(tentId, []);
        }

        public void RemoveTent(int tentId)
        {
            if (!_tents.TryGetValue(tentId, out List<RoomUser> users))
                return;
            foreach (RoomUser user in users.ToList()) {
                if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                    continue;

                user.GetClient().GetHabbo().TentId = 0;
            }

            _tents.Remove(tentId);
        }

        public void AddUserToTent(int tentId, RoomUser user)
        {
            if (user != null && user.GetClient() != null && user.GetClient().GetHabbo() != null) {
                if (!_tents.ContainsKey(tentId))
                    _tents.Add(tentId, []);

                if (!_tents[tentId].Contains(user))
                    _tents[tentId].Add(user);
                user.GetClient().GetHabbo().TentId = tentId;
            }
        }

        public void RemoveUserFromTent(int tentId, RoomUser user)
        {
            if (user != null && user.GetClient() != null && user.GetClient().GetHabbo() != null) {
                if (!_tents.ContainsKey(tentId))
                    _tents.Add(tentId, []);

                _tents[tentId].Remove(user);

                user.GetClient().GetHabbo().TentId = 0;
            }
        }

        public void SendToTent(int id, int tentId, MessageComposer packet)
        {
            if (!_tents.TryGetValue(tentId, out List<RoomUser> value))
                return;

            foreach (RoomUser user in value.ToList()) {
                if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null || user.GetClient().GetHabbo().GetIgnores().IgnoredUserIds().Contains(id) || user.GetClient().GetHabbo().TentId != tentId)
                    continue;

                user.GetClient().SendPacket(packet);
            }
        }

        #endregion

        #region Communication (Packets)

        public void SendPacket(MessageComposer message, bool withRightsOnly = false)
        {
            if (message == null)
                return;

            try {
                RoomUserManager manager = _roomUserManager;
                if (manager == null)
                    return;

                // Enumerating the concurrent dictionary directly is snapshot-safe; the old
                // code allocated a fresh list copy for every single broadcast packet.
                foreach (RoomUser user in manager.GetUserList()) {
                    if (user == null || user.IsBot)
                        continue;

                    if (user.GetClient() == null)
                        continue;

                    if (withRightsOnly && !CheckRights(user.GetClient()))
                        continue;

                    user.GetClient().SendPacket(message);
                }
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }
        }

        public void SendPacketExcept(MessageComposer message, GameClient excludedSession)
        {
            if (message == null)
                return;

            try {
                RoomUserManager manager = _roomUserManager;
                if (manager == null)
                    return;

                foreach (RoomUser user in manager.GetUserList()) {
                    if (user == null || user.IsBot)
                        continue;

                    if (user.GetClient() == null || user.GetClient() == excludedSession)
                        continue;

                    user.GetClient().SendPacket(message);
                }
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }
        }

        public void BroadcastPacket(List<MessageComposer> packets)
        {
            foreach (RoomUser user in _roomUserManager.GetUserList()) {
                if (user == null || user.IsBot)
                    continue;

                if (user.GetClient() == null)
                    continue;

                user.GetClient().SendPacketsAsync(packets);
            }
        }

        public void SendPacket(List<MessageComposer> packets)
        {
            if (packets.Count == 0)
                return;

            BroadcastPacket(packets);
        }

        #endregion

        public void Dispose()
        {
            // Boot everyone still inside to the configured RP home room (room unload / deletion);
            // fall back to the hotel view for anyone without a valid home room configured.
            foreach (RoomUser user in _roomUserManager.GetUserList().ToList()) {
                if (user == null || user.IsBot || user.GetClient() == null)
                    continue;

                if (!Plus.HabboHotel.Roleplay.Utilities.RpHomeRoomService.SendHome(user.GetClient()))
                    user.GetClient().SendPacket(new CloseConnectionComposer());
            }

            if (!mDisposed) {
                IsCrashed = false;
                mDisposed = true;

                /* TODO: Needs reviewing */
                try {
                    if (ProcessTask != null && ProcessTask.IsCompleted)
                        ProcessTask.Dispose();
                } catch {
                }

                if (ActiveTrades.Count > 0)
                    ActiveTrades.Clear();

                TonerData = null;
                MoodlightData = null;

                if (MutedUsers.Count > 0)
                    MutedUsers.Clear();

                if (_tents.Count > 0)
                    _tents.Clear();

                if (UsersWithRights.Count > 0)
                    UsersWithRights.Clear();
                _gameManager?.Dispose();
                _gameManager = null;
                _freeze?.Dispose();
                _freeze = null;
                _soccer?.Dispose();
                _soccer = null;
                _banzai?.Dispose();
                _banzai = null;
                _gamemap?.Dispose();
                _gamemap = null;
                _gameItemHandler?.Dispose();
                _gameItemHandler = null;

                // Room Data?

                TeamBanzai?.Dispose();
                TeamBanzai = null;
                TeamFreeze?.Dispose();
                TeamFreeze = null;
                _roomUserManager?.Dispose();
                _roomUserManager = null;
                _roomItemHandling?.Dispose();
                _roomItemHandling = null;

                if (WordFilterList.Count > 0)
                    WordFilterList.Clear();

                _filterComponent?.Cleanup();
                _wiredComponent?.Cleanup();
                _bansComponent?.Cleanup();
                _tradingComponent?.Cleanup();
            }
        }
    }
}