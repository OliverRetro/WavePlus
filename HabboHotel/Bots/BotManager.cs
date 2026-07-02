using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using log4net;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.AI.Speech;
using Plus.HabboHotel.Rooms.AI.Responses;

namespace Plus.HabboHotel.Bots
{
    public class BotManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(BotManager));

        private readonly List<BotResponse> _responses;
        private readonly Dictionary<int, TaxiBotTemplate> _taxiTemplates;
        private readonly Dictionary<int, List<TaxiPointData>> _taxiPoints;
        private readonly ConcurrentDictionary<int, TaxiRideRequest> _activeTaxiRequests;
        private readonly Random _random;

        public BotManager()
        {
            _responses = new List<BotResponse>();
            _taxiTemplates = new Dictionary<int, TaxiBotTemplate>();
            _taxiPoints = new Dictionary<int, List<TaxiPointData>>();
            _activeTaxiRequests = new ConcurrentDictionary<int, TaxiRideRequest>();
            _random = new Random();
        }

        public void Init()
        {
            if (_responses.Count > 0)
                _responses.Clear();

            if (_taxiTemplates.Count > 0)
                _taxiTemplates.Clear();

            if (_taxiPoints.Count > 0)
                _taxiPoints.Clear();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `bot_ai`,`chat_keywords`,`response_text`,`response_mode`,`response_beverage` FROM `bots_responses`");
                DataTable data = dbClient.GetTable();

                if (data != null) {
                    foreach (DataRow row in data.Rows)
                    {
                        _responses.Add(new BotResponse(Convert.ToString(row["bot_ai"]), Convert.ToString(row["chat_keywords"]), Convert.ToString(row["response_text"]), row["response_mode"].ToString(), Convert.ToString(row["response_beverage"])));
                    }
                }

                dbClient.SetQuery("SELECT `id`,`ai_type`,`walk_mode`,`name`,`motto`,`look`,`gender`,`rotation`,`automatic_chat`,`speaking_interval`,`mix_sentences`,`chat_bubble`,`effect_id` FROM `bots`");
                DataTable bots = dbClient.GetTable();
                if (bots != null) {
                    string configuredBots = PlusEnvironment.GetSettingsManager().TryGetValue("rp.taxi.bots");
                    Dictionary<int, int> probabilities = ParseTaxiBotProbabilities(configuredBots);

                    foreach (DataRow row in bots.Rows)
                    {
                        int botId = Convert.ToInt32(row["id"]);
                        if (!probabilities.ContainsKey(botId))
                            continue;

                        dbClient.SetQuery("SELECT `text` FROM `bots_speech` WHERE `bot_id` = @botId");
                        dbClient.AddParameter("botId", botId);
                        DataTable speechTable = dbClient.GetTable();
                        List<RandomSpeech> speeches = new();
                        if (speechTable != null) {
                            foreach (DataRow speechRow in speechTable.Rows)
                            {
                                speeches.Add(new RandomSpeech(Convert.ToString(speechRow["text"]), botId));
                            }
                        }

                        _taxiTemplates[botId] = new TaxiBotTemplate(
                            botId,
                            Convert.ToString(row["ai_type"]),
                            Convert.ToString(row["walk_mode"]),
                            Convert.ToString(row["name"]),
                            Convert.ToString(row["motto"]),
                            Convert.ToString(row["look"]),
                            Convert.ToString(row["gender"]),
                            Convert.ToInt32(row["rotation"]),
                            Convert.ToBoolean(row["automatic_chat"]),
                            Convert.ToInt32(row["speaking_interval"]),
                            PlusEnvironment.EnumToBool(Convert.ToString(row["mix_sentences"])),
                            Convert.ToInt32(row["chat_bubble"]),
                            Convert.ToInt32(row["effect_id"]),
                            speeches);
                    }
                }

                dbClient.SetQuery("SELECT `room_id`,`x`,`y`,`z`,`rotation`,`cost`,`delay` FROM `room_taxi_points`");
                DataTable taxiPoints = dbClient.GetTable();
                if (taxiPoints != null) {
                    foreach (DataRow row in taxiPoints.Rows)
                    {
                        int roomId = Convert.ToInt32(row["room_id"]);
                        if (!_taxiPoints.TryGetValue(roomId, out List<TaxiPointData> roomPoints)) {
                            roomPoints = new List<TaxiPointData>();
                            _taxiPoints[roomId] = roomPoints;
                        }

                        roomPoints.Add(new TaxiPointData(roomId, Convert.ToInt32(row["x"]), Convert.ToInt32(row["y"]), Convert.ToDouble(row["z"]), Convert.ToInt32(row["rotation"]), Convert.ToInt32(row["cost"]), Convert.ToInt32(row["delay"])));
                    }
                }
            }
        }

        public void OnCycle()
        {
            foreach (TaxiRideRequest request in _activeTaxiRequests.Values.ToList())
            {
                try
                {
                    ProcessRequest(request);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    CancelTaxi(request.UserId, false);
                }
            }
        }

        public BotResponse GetResponse(BotAIType type, string message)
        {
            foreach (BotResponse response in _responses.Where(x => x.AiType == type).ToList())
            {
                if (response.KeywordMatched(message)) {
                    return response;
                }
            }

            return null;
        }

        public bool TryGetTaxiPoints(int roomId, out List<TaxiPointData> points)
        {
            return _taxiPoints.TryGetValue(roomId, out points);
        }

        public void AddTaxiPoint(TaxiPointData point)
        {
            if (!_taxiPoints.TryGetValue(point.RoomId, out List<TaxiPointData> roomPoints)) {
                roomPoints = new List<TaxiPointData>();
                _taxiPoints[point.RoomId] = roomPoints;
            }

            roomPoints.Add(point);

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `room_taxi_points` (`room_id`,`x`,`y`,`z`,`rotation`,`cost`,`delay`) VALUES (@roomId,@x,@y,@z,@rotation,@cost,@delay)");
                dbClient.AddParameter("x", point.X);
                dbClient.AddParameter("y", point.Y);
                dbClient.AddParameter("z", point.Z);
                dbClient.AddParameter("rotation", point.Rotation);
                dbClient.AddParameter("cost", point.Cost);
                dbClient.AddParameter("delay", point.Delay);
                dbClient.AddParameter("roomId", point.RoomId);
                dbClient.RunQuery();
            }
        }

        public bool TryGetTaxiTemplate(int botId, out TaxiBotTemplate template)
        {
            return _taxiTemplates.TryGetValue(botId, out template);
        }

        public bool HasActiveTaxi(int userId)
        {
            return _activeTaxiRequests.ContainsKey(userId);
        }

        public bool TryCallTaxi(GameClient session, int targetRoomId, out string error)
        {
            error = string.Empty;
            if (session?.GetHabbo() == null || session.GetHabbo().CurrentRoom == null) {
                error = "You cannot call a taxi right now.";
                return false;
            }

            int userId = session.GetHabbo().Id;
            if (_activeTaxiRequests.ContainsKey(userId)) {
                error = "You already have a taxi on the way.";
                return false;
            }

            Room sourceRoom = session.GetHabbo().CurrentRoom;
            if (targetRoomId == sourceRoom.Id) {
                error = "You cannot take a taxi to the room you are already in.";
                return false;
            }

            if (!_taxiPoints.TryGetValue(sourceRoom.Id, out List<TaxiPointData> sourcePoints) || sourcePoints.Count == 0) {
                error = "There is no taxi pickup point configured for this room.";
                return false;
            }

            if (!_taxiPoints.TryGetValue(targetRoomId, out List<TaxiPointData> targetPoints) || targetPoints.Count == 0) {
                error = "That room is not serviced by taxis.";
                return false;
            }

            TaxiPointData sourcePoint = sourcePoints[_random.Next(sourcePoints.Count)];
            TaxiPointData targetPoint = targetPoints[_random.Next(targetPoints.Count)];

            if (!RoomFactory.TryGetData(targetRoomId, out RoomData targetRoomData)) {
                error = "That room does not exist.";
                return false;
            }

            if (targetRoomData.Access != RoomAccess.Open && !session.GetHabbo().GetPermissions().HasRight("room_enter_locked")) {
                error = "You can only take a taxi to open rooms.";
                return false;
            }

            int templateBotId = PickTaxiTemplateId();
            if (templateBotId == 0 || !_taxiTemplates.ContainsKey(templateBotId)) {
                error = "No taxi drivers are configured.";
                return false;
            }

            if (session.GetHabbo().Credits < sourcePoint.Cost) {
                error = "You do not have enough credits for this taxi.";
                return false;
            }

            session.GetHabbo().Credits -= sourcePoint.Cost;
            session.SendPacket(new Plus.Communication.Packets.Outgoing.Inventory.Purse.CreditBalanceComposer(session.GetHabbo().Credits));

            _activeTaxiRequests[userId] = new TaxiRideRequest
            {
                UserId = userId,
                SourceRoomId = sourceRoom.Id,
                TargetRoomId = targetRoomId,
                TaxiBotTemplateId = templateBotId,
                ArrivalAtUtc = DateTime.UtcNow.AddSeconds(sourcePoint.Delay),
                SpawnX = sourcePoint.X,
                SpawnY = sourcePoint.Y,
                SpawnZ = sourcePoint.Z,
                SpawnRotation = sourcePoint.Rotation,
                ExitSpawnX = targetPoint.X,
                ExitSpawnY = targetPoint.Y,
                ExitSpawnZ = targetPoint.Z,
                ExitSpawnRotation = targetPoint.Rotation,
                SourceCost = sourcePoint.Cost,
                TaxiVirtualId = 0,
                ExitTaxiVirtualId = 0,
                Spawned = false,
                PassengerBoarded = false,
                PassengerBoardedAtUtc = DateTime.MinValue,
                Arrived = false,
                EscortTicks = 0,
                ArrivalMessageSent = false,
                ArrivalMessageAtUtc = DateTime.MinValue,
                ExitRemovalAtUtc = DateTime.MinValue,
                ExitTargetX = 0,
                ExitTargetY = 0,
                ExitTimeoutAtUtc = DateTime.MinValue
            };

            return true;
        }

        public void CancelTaxi(int userId, bool refund)
        {
            if (!_activeTaxiRequests.TryRemove(userId, out TaxiRideRequest request))
                return;

            if (request.TaxiUser != null && request.TaxiUser.GetRoom() != null) {
                request.TaxiUser.GetRoom().GetRoomUserManager().RemoveBot(request.TaxiUser.VirtualId, true);
            }

            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
            if (refund && client?.GetHabbo() != null) {
                client.GetHabbo().Credits += request.SourceCost;
                client.SendPacket(new Plus.Communication.Packets.Outgoing.Inventory.Purse.CreditBalanceComposer(client.GetHabbo().Credits));
            }
        }

        public void HandleUserRoomLeave(GameClient client, Room room)
        {
            if (client?.GetHabbo() == null || room == null)
                return;

            if (_activeTaxiRequests.TryGetValue(client.GetHabbo().Id, out TaxiRideRequest request) && request.SourceRoomId == room.Id && !request.PassengerBoarded)
                CancelTaxi(client.GetHabbo().Id, true);
        }

        public void TryHandleTaxiArrival(GameClient session)
        {
            if (session?.GetHabbo() == null)
                return;

            if (!_activeTaxiRequests.TryGetValue(session.GetHabbo().Id, out TaxiRideRequest request) || !request.PassengerBoarded)
                return;

            Room room = session.GetHabbo().CurrentRoom;
            if (room == null || room.Id != request.TargetRoomId)
                return;

            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
                return;

            if (!room.GetGameMap().ValidTile(request.ExitSpawnX, request.ExitSpawnY) || !room.GetGameMap().SquareIsOpen(request.ExitSpawnX, request.ExitSpawnY, false)) {
                CancelTaxi(session.GetHabbo().Id, false);
                return;
            }

            user.SetPos(request.ExitSpawnX, request.ExitSpawnY, request.ExitSpawnZ);
            user.SetRot(request.ExitSpawnRotation, false);
            room.GetRoomUserManager().UpdateUserStatus(user, false);

            SpawnTaxiAtDestination(room, request);

            if (request.TaxiUser != null && request.TaxiUser.GetRoom() != null)
                request.TaxiUser.GetRoom().GetRoomUserManager().RemoveBot(request.TaxiUser.VirtualId, true);

            request.TaxiUser = null;
            request.TaxiVirtualId = 0;
            request.Arrived = true;
        }

        private void ProcessRequest(TaxiRideRequest request)
        {
            HandleDelayedTaxiSpeech(request);
            HandleExitTaxiRemoval(request);

            if (request.Arrived) {
                if (request.ExitTaxiVirtualId <= 0)
                    _activeTaxiRequests.TryRemove(request.UserId, out _);

                return;
            }

            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(request.UserId);
            if (client?.GetHabbo() == null) {
                CancelTaxi(request.UserId, false);
                return;
            }

            if (!request.Spawned) {
                if (client.GetHabbo().CurrentRoomId != request.SourceRoomId) {
                    CancelTaxi(request.UserId, true);
                    return;
                }

                if (DateTime.UtcNow >= request.ArrivalAtUtc) {
                    SpawnTaxi(request, client);
                }

                return;
            }

            if (request.TaxiUser == null || request.TaxiUser.GetRoom() == null) {
                CancelTaxi(request.UserId, false);
                return;
            }

            if (!request.PassengerBoarded) {
                RoomUser rider = client.GetHabbo().CurrentRoom?.GetRoomUserManager().GetRoomUserByHabbo(request.UserId);
                if (rider == null) {
                    CancelTaxi(request.UserId, false);
                    return;
                }

                request.TaxiUser.BotData.ForcedUserTargetMovement = request.UserId;
                double distance = Math.Sqrt(Math.Pow(request.TaxiUser.X - rider.X, 2) + Math.Pow(request.TaxiUser.Y - rider.Y, 2));
                if (distance <= 1.5) {
                    request.PassengerBoarded = true;
                    request.PassengerBoardedAtUtc = DateTime.UtcNow;
                    request.TaxiUser.BotData.ForcedUserTargetMovement = 0;
                    client.GetHabbo().PrepareRoom(request.TargetRoomId, string.Empty);
                }

                return;
            }

            if (!request.Arrived) {
                if (client.GetHabbo().CurrentRoomId == request.TargetRoomId)
                    return;

                if (request.PassengerBoardedAtUtc != DateTime.MinValue && DateTime.UtcNow <= request.PassengerBoardedAtUtc.AddSeconds(5))
                    return;

                CancelTaxi(request.UserId, false);
                return;
            }

            request.EscortTicks++;
            if (request.EscortTicks > 20)
                CancelTaxi(request.UserId, false);
        }

        private void SpawnTaxi(TaxiRideRequest request, GameClient client)
        {
            if (!_taxiTemplates.TryGetValue(request.TaxiBotTemplateId, out TaxiBotTemplate template)) {
                CancelTaxi(request.UserId, true);
                return;
            }

            Room room = client.GetHabbo().CurrentRoom;
            if (room == null || room.Id != request.SourceRoomId) {
                CancelTaxi(request.UserId, true);
                return;
            }

            List<RandomSpeech> speeches = new();
            RoomBot bot = new RoomBot(template.BotId, room.Id, template.AiType, "freeroam", template.Name, template.Motto, template.Look, request.SpawnX, request.SpawnY, request.SpawnZ, request.SpawnRotation, 0, 0, 0, 0, ref speeches, template.Gender, 0, 0, false, template.SpeakingInterval, template.MixSentences, template.ChatBubble, template.EffectId);
            RoomUser botUser = room.GetRoomUserManager().DeployBot(bot, null);
            request.TaxiUser = botUser;
            request.TaxiVirtualId = botUser.VirtualId;
            request.Spawned = true;
            request.ArrivalMessageAtUtc = DateTime.UtcNow.AddSeconds(1);
            request.TaxiUser.BotData.ForcedUserTargetMovement = request.UserId;
        }

        private void SpawnTaxiAtDestination(Room room, TaxiRideRequest request)
        {
            if (!_taxiTemplates.TryGetValue(request.TaxiBotTemplateId, out TaxiBotTemplate template))
                return;

            List<RandomSpeech> speeches = new();
            RoomBot bot = new RoomBot(template.BotId, room.Id, template.AiType, "freeroam", template.Name, template.Motto, template.Look, request.ExitSpawnX, request.ExitSpawnY, request.ExitSpawnZ, request.ExitSpawnRotation, 0, 0, 0, 0, ref speeches, template.Gender, 0, 0, false, template.SpeakingInterval, template.MixSentences, template.ChatBubble, template.EffectId);
            RoomUser botUser = room.GetRoomUserManager().DeployBot(bot, null);
            botUser.Chat("Thanks for business!", template.ChatBubble);
            request.ExitTaxiVirtualId = botUser.VirtualId;

            if (_taxiPoints.TryGetValue(room.Id, out List<TaxiPointData> roomPoints) && roomPoints.Count > 1) {
                List<TaxiPointData> otherPoints = roomPoints.Where(x => x.X != request.ExitSpawnX || x.Y != request.ExitSpawnY || x.Rotation != request.ExitSpawnRotation).ToList();
                if (otherPoints.Count > 0) {
                    TaxiPointData furthestPoint = otherPoints.OrderByDescending(x => Math.Sqrt(Math.Pow(x.X - request.ExitSpawnX, 2) + Math.Pow(x.Y - request.ExitSpawnY, 2))).First();
                    Point origin = new(request.ExitSpawnX, request.ExitSpawnY);
                    Point target = new(furthestPoint.X, furthestPoint.Y);
                    int distance = Math.Abs(target.X - origin.X) + Math.Abs(target.Y - origin.Y);

                    if (distance < 15 && botUser.BotAI != null) {
                        Point? farAway = botUser.BotAI.GetFarAwayWalkableSquare(origin, 15);
                        if (farAway.HasValue) {
                            target = farAway.Value;
                            distance = Math.Abs(target.X - origin.X) + Math.Abs(target.Y - origin.Y);
                        }
                    }

                    botUser.BotData.ForcedMovement = true;
                    botUser.BotData.TargetCoordinate = target;
                    botUser.MoveTo(target.X, target.Y);
                    request.ExitTargetX = target.X;
                    request.ExitTargetY = target.Y;
                    request.ExitRemovalAtUtc = DateTime.UtcNow.AddSeconds(distance >= 15 ? 5 : 3);
                    request.ExitTimeoutAtUtc = DateTime.UtcNow.AddSeconds(distance >= 15 ? 6 : 3);
                    return;
                }
            }

            room.GetRoomUserManager().RemoveBot(botUser.VirtualId, true);
            request.ExitTaxiVirtualId = 0;
        }

        private void HandleDelayedTaxiSpeech(TaxiRideRequest request)
        {
            if (request.ArrivalMessageSent || request.ArrivalMessageAtUtc == DateTime.MinValue || request.TaxiUser == null)
                return;

            if (DateTime.UtcNow < request.ArrivalMessageAtUtc)
                return;

            if (!_taxiTemplates.TryGetValue(request.TaxiBotTemplateId, out TaxiBotTemplate template))
                return;

            string arrivalSpeech = template.Speeches.FirstOrDefault()?.Message;
            if (!string.IsNullOrWhiteSpace(arrivalSpeech) && request.TaxiUser.GetRoom() != null)
                request.TaxiUser.Chat(arrivalSpeech, template.ChatBubble);

            request.ArrivalMessageSent = true;
            request.ArrivalMessageAtUtc = DateTime.MinValue;
        }

        private void HandleExitTaxiRemoval(TaxiRideRequest request)
        {
            if (request.ExitTaxiVirtualId <= 0)
                return;

            if (PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(request.TargetRoomId, out Room room)) {
                RoomUser exitTaxi = room.GetRoomUserManager().GetRoomUserByVirtualId(request.ExitTaxiVirtualId);
                if (exitTaxi != null) {
                    bool reachedTarget = request.ExitTargetX > 0 && request.ExitTargetY > 0 && exitTaxi.X == request.ExitTargetX && exitTaxi.Y == request.ExitTargetY;
                    bool timedOut = request.ExitTimeoutAtUtc != DateTime.MinValue && DateTime.UtcNow >= request.ExitTimeoutAtUtc;
                    bool delayedEnough = request.ExitRemovalAtUtc != DateTime.MinValue && DateTime.UtcNow >= request.ExitRemovalAtUtc;

                    if (!reachedTarget && !timedOut && !delayedEnough)
                        return;

                    room.GetRoomUserManager().RemoveBot(exitTaxi.VirtualId, true);
                }
            }

            request.ExitTaxiVirtualId = 0;
            request.ExitRemovalAtUtc = DateTime.MinValue;
            request.ExitTargetX = 0;
            request.ExitTargetY = 0;
            request.ExitTimeoutAtUtc = DateTime.MinValue;
        }

        private int PickTaxiTemplateId()
        {
            Dictionary<int, int> probabilities = ParseTaxiBotProbabilities(PlusEnvironment.GetSettingsManager().TryGetValue("rp.taxi.bots"));
            if (probabilities.Count == 0)
                return 0;

            int roll = _random.Next(1, 101);
            int current = 0;
            foreach (KeyValuePair<int, int> entry in probabilities)
            {
                current += entry.Value;
                if (roll <= current)
                    return entry.Key;
            }

            return probabilities.Keys.FirstOrDefault();
        }

        private static Dictionary<int, int> ParseTaxiBotProbabilities(string value)
        {
            Dictionary<int, int> result = new();
            if (string.IsNullOrWhiteSpace(value) || value == "0")
                return result;

            foreach (string entry in value.Split(';'))
            {
                string[] parts = entry.Split(':');
                if (parts.Length != 2)
                    continue;

                if (!int.TryParse(parts[0], out int botId) || !int.TryParse(parts[1], out int probability) || botId <= 0 || probability <= 0)
                    continue;

                result[botId] = probability;
            }

            return result;
        }

        public static bool IsNearTaxiPole(Room room, RoomUser user)
        {
            if (room?.GetRoomItemHandler() == null || user == null)
                return false;

            foreach (Item item in room.GetRoomItemHandler().GetFloor)
            {
                if (item?.GetBaseItem()?.InteractionType != InteractionType.TaxiPole)
                    continue;

                bool ownedByUser = item.UserId == user.UserId;
                double distance = Math.Sqrt(Math.Pow(item.GetX - user.X, 2) + Math.Pow(item.GetY - user.Y, 2));
                if (ownedByUser || distance <= 3.5)
                    return true;
            }

            return false;
        }
    }
}
