using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Core;
using Plus.Database.Interfaces;
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

        public Room(RoomData data)
            : base(data)
        {
            IsLagging = 0;
            Unloaded = false;
            IdleTime = 0;

            RoomMuted = false;

            MutedUsers = new Dictionary<int, double>();
            _tents = new Dictionary<int, List<RoomUser>>();

            _gamemap = new Gamemap(this, data.Model);
            _roomItemHandling = new RoomItemHandling(this);

            _roomUserManager = new RoomUserManager(this);
            _filterComponent = new FilterComponent(this);
            _wiredComponent = new WiredComponent(this);
            _bansComponent = new BansComponent(this);
            _tradingComponent = new TradingComponent(this);
            ActiveTrades = new ArrayList();

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

        public int UserCount => _roomUserManager.GetRoomUsers().Count;

        public int RoomId => Id;

        public bool CanTradeInRoom => true;

        public Gamemap GetGameMap()
        {
            return _gamemap;
        }

        public RoomItemHandling GetRoomItemHandler()
        {
            if (_roomItemHandling == null)
            {
                _roomItemHandling = new RoomItemHandling(this);
            }

            return _roomItemHandling;
        }

        public RoomUserManager GetRoomUserManager()
        {
            return _roomUserManager;
        }

        public Soccer GetSoccer()
        {
            if (_soccer == null)
                _soccer = new Soccer(this);

            return _soccer;
        }

        public TeamManager GetTeamManagerForBanzai()
        {
            if (TeamBanzai == null)
                TeamBanzai = TeamManager.CreateTeam("banzai");
            return TeamBanzai;
        }

        public TeamManager GetTeamManagerForFreeze()
        {
            if (TeamFreeze == null)
                TeamFreeze = TeamManager.CreateTeam("freeze");
            return TeamFreeze;
        }

        public BattleBanzai GetBanzai()
        {
            if (_banzai == null)
                _banzai = new BattleBanzai(this);
            return _banzai;
        }

        public Freeze GetFreeze()
        {
            if (_freeze == null)
                _freeze = new Freeze(this);
            return _freeze;
        }

        public GameManager GetGameManager()
        {
            if (_gameManager == null)
                _gameManager = new GameManager(this);
            return _gameManager;
        }

        public GameItemHandler GetGameItemHandler()
        {
            if (_gameItemHandler == null)
                _gameItemHandler = new GameItemHandler(this);
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
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`room_id`,`name`,`motto`,`look`,`x`,`y`,`z`,`rotation`,`gender`,`user_id`,`ai_type`,`walk_mode`,`automatic_chat`,`speaking_interval`,`mix_sentences`,`chat_bubble`,`effect_id` FROM `bots` WHERE `room_id` = '" + RoomId + "' AND `ai_type` != 'pet'");
                DataTable data = dbClient.GetTable();
                if (data == null)
                    return;

                foreach (DataRow bot in data.Rows)
                {
                    dbClient.SetQuery("SELECT `text` FROM `bots_speech` WHERE `bot_id` = '" + Convert.ToInt32(bot["id"]) + "'");
                    DataTable botSpeech = dbClient.GetTable();

                    List<RandomSpeech> speeches = new();

                    foreach (DataRow speech in botSpeech.Rows)
                    {
                        speeches.Add(new RandomSpeech(Convert.ToString(speech["text"]), Convert.ToInt32(bot["id"])));
                    }

                    _roomUserManager.DeployBot(new RoomBot(Convert.ToInt32(bot["id"]), Convert.ToInt32(bot["room_id"]), Convert.ToString(bot["ai_type"]), Convert.ToString(bot["walk_mode"]), Convert.ToString(bot["name"]), Convert.ToString(bot["motto"]), Convert.ToString(bot["look"]), int.Parse(bot["x"].ToString()), int.Parse(bot["y"].ToString()), int.Parse(bot["z"].ToString()), int.Parse(bot["rotation"].ToString()), 0, 0, 0, 0, ref speeches, Convert.ToString(bot["gender"]), 0, Convert.ToInt32(bot["user_id"].ToString()), Convert.ToBoolean(bot["automatic_chat"]), Convert.ToInt32(bot["speaking_interval"]), PlusEnvironment.EnumToBool(bot["mix_sentences"].ToString()), Convert.ToInt32(bot["chat_bubble"]), Convert.ToInt32(bot["effect_id"])), null);
                }
            }
        }

        public void InitPets()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`user_id`,`room_id`,`name`,`x`,`y`,`z` FROM `bots` WHERE `room_id` = '" + RoomId + "' AND `ai_type` = 'pet'");
                DataTable data = dbClient.GetTable();

                if (data == null)
                    return;

                foreach (DataRow row in data.Rows)
                {
                    dbClient.SetQuery("SELECT `type`,`race`,`color`,`experience`,`energy`,`nutrition`,`respect`,`createstamp`,`have_saddle`,`anyone_ride`,`hairdye`,`pethair`,`gnome_clothing` FROM `bots_petdata` WHERE `id` = '" + row[0] + "' LIMIT 1");
                    DataRow mRow = dbClient.GetRow();
                    if (mRow == null)
                        continue;

                    Pet pet = new(Convert.ToInt32(row["id"]), Convert.ToInt32(row["user_id"]), Convert.ToInt32(row["room_id"]), Convert.ToString(row["name"]), Convert.ToInt32(mRow["type"]), Convert.ToString(mRow["race"]),
                        Convert.ToString(mRow["color"]), Convert.ToInt32(mRow["experience"]), Convert.ToInt32(mRow["energy"]), Convert.ToInt32(mRow["nutrition"]), Convert.ToInt32(mRow["respect"]), Convert.ToDouble(mRow["createstamp"]), Convert.ToInt32(row["x"]), Convert.ToInt32(row["y"]),
                        Convert.ToDouble(row["z"]), Convert.ToInt32(mRow["have_saddle"]), Convert.ToInt32(mRow["anyone_ride"]), Convert.ToInt32(mRow["hairdye"]), Convert.ToInt32(mRow["pethair"]), Convert.ToString(mRow["gnome_clothing"]));

                    var rndSpeechList = new List<RandomSpeech>();

                    _roomUserManager.DeployBot(new RoomBot(pet.PetId, RoomId, "pet", "freeroam", pet.Name, "", pet.Look, pet.X, pet.Y, Convert.ToInt32(pet.Z), 0, 0, 0, 0, 0, ref rndSpeechList, "", 0, pet.OwnerId, false, 0, false, 0, 0), pet);
                }
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
            UsersWithRights = new List<int>();
            if (Group != null)
                return;

            DataTable data = null;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT room_rights.user_id FROM room_rights WHERE room_id = @roomid");
                dbClient.AddParameter("roomid", Id);
                data = dbClient.GetTable();
            }

            if (data != null)
            {
                foreach (DataRow row in data.Rows)
                {
                    UsersWithRights.Add(Convert.ToInt32(row["user_id"]));
                }
            }
        }

        private void LoadFilter()
        {
            WordFilterList = new List<string>();

            DataTable data;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `room_filter` WHERE `room_id` = @roomid;");
                dbClient.AddParameter("roomid", Id);
                data = dbClient.GetTable();
            }

            if (data == null)
                return;

            foreach (DataRow row in data.Rows)
            {
                WordFilterList.Add(Convert.ToString(row["word"]));
            }
        }

        public bool CheckRights(GameClient session)
        {
            return CheckRights(session, false);
        }

        public bool CheckRights(GameClient session, bool requireOwnership, bool checkForGroups = false)
        {
            try
            {
                if (session == null || session.GetHabbo() == null)
                    return false;

                if (session.GetHabbo().Username == OwnerName && Type == "private")
                    return true;

                if (session.GetHabbo().GetPermissions().HasRight("room_any_owner"))
                    return true;

                if (!requireOwnership && Type == "private")
                {
                    if (session.GetHabbo().GetPermissions().HasRight("room_any_rights"))
                        return true;

                    if (UsersWithRights.Contains(session.GetHabbo().Id))
                        return true;
                }

                if (checkForGroups && Type == "private")
                {
                    if (Group == null)
                        return false;

                    if (Group.IsAdmin(session.GetHabbo().Id))
                        return true;

                    if (Group.AdminOnlyDeco == 0)
                    {
                        if (Group.IsAdmin(session.GetHabbo().Id))
                            return true;
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }

            return false;
        }

        public void OnUserShoot(RoomUser user, Item ball)
        {
            Func<Item, bool> predicate = null;
            string key = null;
            foreach (Item item in GetRoomItemHandler().GetFurniObjects(ball.GetX, ball.GetY).ToList())
            {
                if (item.GetBaseItem().ItemName.StartsWith("fball_goal_"))
                {
                    key = item.GetBaseItem().ItemName.Split(new[] {'_'})[2];
                    user.UnIdle();
                    user.DanceId = 0;


                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(user.GetClient(), "ACH_FootballGoalScored", 1);

                    SendPacket(new ActionComposer(user.VirtualId, 1));
                }
            }

            if (key != null)
            {
                if (predicate == null)
                {
                    predicate = p => p.GetBaseItem().ItemName == "fball_score_" + key;
                }

                foreach (Item item2 in GetRoomItemHandler().GetFloor.Where(predicate).ToList())
                {
                    if (item2.GetBaseItem().ItemName == "fball_score_" + key)
                    {
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

            try
            {
                if (GetRoomUserManager().GetRoomUsers().Count == 0)
                    IdleTime++;
                else if (IdleTime > 0)
                    IdleTime = 0;

                if (HasActivePromotion && Promotion.HasExpired)
                {
                    EndPromotion();
                }

                if (IdleTime >= 60 && !HasActivePromotion)
                {
                    PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(Id);
                    return;
                }

                try
                {
                    GetRoomItemHandler().OnCycle();
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                }

                try
                {
                    GetRoomUserManager().OnCycle();
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                }

                #region Status Updates

                try
                {
                    GetRoomUserManager().SerializeStatusUpdates();
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                }

                #endregion

                #region Game Item Cycle

                try
                {
                    _gameItemHandler?.OnCycle();
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                }

                #endregion

                try
                {
                    GetWired().OnCycle();
                }
                catch (Exception e)
                {
                    ExceptionLogger.LogException(e);
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
                OnRoomCrash(e);
            }
        }

        private void OnRoomCrash(Exception e)
        {
            try
            {
                foreach (RoomUser user in _roomUserManager.GetRoomUsers().ToList())
                {
                    if (user == null || user.GetClient() == null)
                        continue;

                    user.GetClient().SendNotification("Sorry, it appears that room has crashed!"); //Unhandled exception in room: " + e);

                    try
                    {
                        GetRoomUserManager().RemoveUserFromRoom(user.GetClient(), true);
                    }
                    catch (Exception e2)
                    {
                        ExceptionLogger.LogException(e2);
                    }
                }
            }
            catch (Exception e3)
            {
                ExceptionLogger.LogException(e3);
            }

            IsCrashed = true;
            PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(Id);
        }


        public bool CheckMute(GameClient session)
        {
            if (MutedUsers.ContainsKey(session.GetHabbo().Id))
            {
                if (MutedUsers[session.GetHabbo().Id] < PlusEnvironment.GetUnixTimestamp())
                {
                    MutedUsers.Remove(session.GetHabbo().Id);
                }
                else
                {
                    return true;
                }
            }

            if (session.GetHabbo().TimeMuted > 0 || RoomMuted && session.GetHabbo().Username != OwnerName)
                return true;

            return false;
        }

        public void SendObjects(GameClient session, Room room)
        {
            session.SendPacket(new HeightMapComposer(room));
            session.SendPacket(new FloorHeightMapComposer(GetGameMap().Model.GetRelativeHeightmap(), GetGameMap().StaticModel.WallHeight));

            foreach (RoomUser user in _roomUserManager.GetUserList().ToList())
            {
                if (user == null)
                    continue;

                session.SendPacket(new UsersComposer(user));

                if (user.IsBot && user.BotData.DanceId > 0)
                    session.SendPacket(new DanceComposer(user.VirtualId, user.BotData.DanceId));
                else if (!user.IsBot && !user.IsPet && user.IsDancing)
                    session.SendPacket(new DanceComposer(user.VirtualId, user.DanceId));

                if (user.IsBot && user.BotData.EffectId > 0)
                    session.SendPacket(new AvatarEffectComposer(user.VirtualId, user.BotData.EffectId));

                if (user.IsAsleep)
                    session.SendPacket(new SleepComposer(user.VirtualId, true));

                if (user.CarryItemId > 0 && user.CarryTimer > 0)
                    session.SendPacket(new CarryObjectComposer(user.VirtualId, user.CarryItemId));

                if (!user.IsBot && !user.IsPet && user.CurrentEffect > 0)
                    session.SendPacket(new AvatarEffectComposer(user.VirtualId, user.CurrentEffect));
            }

            session.SendPacket(new UserUpdateComposer(_roomUserManager.GetUserList().ToList()));
            session.SendPacket(new ObjectsComposer(GetRoomItemHandler().GetFloor.ToArray(), this));
            session.SendPacket(new ItemsComposer(GetRoomItemHandler().GetWall.ToArray(), this));
        }

        #region Tents

        public void AddTent(int tentId)
        {
            if (_tents.ContainsKey(tentId))
                _tents.Remove(tentId);

            _tents.Add(tentId, new List<RoomUser>());
        }

        public void RemoveTent(int tentId)
        {
            if (!_tents.ContainsKey(tentId))
                return;

            List<RoomUser> users = _tents[tentId];
            foreach (RoomUser user in users.ToList())
            {
                if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                    continue;

                user.GetClient().GetHabbo().TentId = 0;
            }

            if (_tents.ContainsKey(tentId))
                _tents.Remove(tentId);
        }

        public void AddUserToTent(int tentId, RoomUser user)
        {
            if (user != null && user.GetClient() != null && user.GetClient().GetHabbo() != null)
            {
                if (!_tents.ContainsKey(tentId))
                    _tents.Add(tentId, new List<RoomUser>());

                if (!_tents[tentId].Contains(user))
                    _tents[tentId].Add(user);
                user.GetClient().GetHabbo().TentId = tentId;
            }
        }

        public void RemoveUserFromTent(int tentId, RoomUser user)
        {
            if (user != null && user.GetClient() != null && user.GetClient().GetHabbo() != null)
            {
                if (!_tents.ContainsKey(tentId))
                    _tents.Add(tentId, new List<RoomUser>());

                if (_tents[tentId].Contains(user))
                    _tents[tentId].Remove(user);

                user.GetClient().GetHabbo().TentId = 0;
            }
        }

        public void SendToTent(int id, int tentId, MessageComposer packet)
        {
            if (!_tents.ContainsKey(tentId))
                return;

            foreach (RoomUser user in _tents[tentId].ToList())
            {
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

            try
            {
                List<RoomUser> users = _roomUserManager.GetUserList().ToList();

                if (_roomUserManager == null)
                    return;

                foreach (RoomUser user in users)
                {
                    if (user == null || user.IsBot)
                        continue;

                    if (user.GetClient() == null)
                        continue;

                    if (withRightsOnly && !CheckRights(user.GetClient()))
                        continue;

                    user.GetClient().SendPacket(message);
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        public void SendPacketExcept(MessageComposer message, GameClient excludedSession)
        {
            if (message == null)
                return;

            try
            {
                List<RoomUser> users = _roomUserManager.GetUserList().ToList();

                if (_roomUserManager == null)
                    return;

                foreach (RoomUser user in users)
                {
                    if (user == null || user.IsBot)
                        continue;

                    if (user.GetClient() == null || user.GetClient() == excludedSession)
                        continue;

                    user.GetClient().SendPacket(message);
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        public void BroadcastPacket(List<MessageComposer> packets)
        {
            foreach (RoomUser user in _roomUserManager.GetUserList().ToList())
            {
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
            SendPacket(new CloseConnectionComposer());

            if (!mDisposed)
            {
                IsCrashed = false;
                mDisposed = true;

                /* TODO: Needs reviewing */
                try
                {
                    if (ProcessTask != null && ProcessTask.IsCompleted)
                        ProcessTask.Dispose();
                }
                catch
                {
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

                if (_gameManager != null)
                {
                    _gameManager.Dispose();
                    _gameManager = null;
                }

                if (_freeze != null)
                {
                    _freeze.Dispose();
                    _freeze = null;
                }

                if (_soccer != null)
                {
                    _soccer.Dispose();
                    _soccer = null;
                }

                if (_banzai != null)
                {
                    _banzai.Dispose();
                    _banzai = null;
                }

                if (_gamemap != null)
                {
                    _gamemap.Dispose();
                    _gamemap = null;
                }

                if (_gameItemHandler != null)
                {
                    _gameItemHandler.Dispose();
                    _gameItemHandler = null;
                }

                // Room Data?

                if (TeamBanzai != null)
                {
                    TeamBanzai.Dispose();
                    TeamBanzai = null;
                }

                if (TeamFreeze != null)
                {
                    TeamFreeze.Dispose();
                    TeamFreeze = null;
                }

                if (_roomUserManager != null)
                {
                    _roomUserManager.Dispose();
                    _roomUserManager = null;
                }

                if (_roomItemHandling != null)
                {
                    _roomItemHandling.Dispose();
                    _roomItemHandling = null;
                }

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
