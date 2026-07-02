using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Core;
using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.Games.Teams;
using Plus.HabboHotel.Rooms.PathFinding;
using Plus.HabboHotel.Rooms.Trading;
using Plus.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Plus.HabboHotel.Rooms
{
    public class RoomUserManager
    {
        private Room _room;
        private ConcurrentDictionary<int, RoomUser> _users;
        private ConcurrentDictionary<int, RoomUser> _bots;
        private ConcurrentDictionary<int, RoomUser> _pets;

        private int _primaryPrivateUserId;
        private int _secondaryPrivateUserId;

        public int UserCount;

        public RoomUserManager(Room room)
        {
            _room = room;
            _users = new ConcurrentDictionary<int, RoomUser>();
            _pets = new ConcurrentDictionary<int, RoomUser>();
            _bots = new ConcurrentDictionary<int, RoomUser>();

            _primaryPrivateUserId = 0;
            _secondaryPrivateUserId = 0;

            PetCount = 0;
            UserCount = 0;
        }

        public RoomUser DeployBot(RoomBot bot, Pet pet)
        {
            RoomUser user = new(0, _room.RoomId, _primaryPrivateUserId++, _room);
            bot.VirtualId = _primaryPrivateUserId;

            int personalId = _secondaryPrivateUserId++;
            user.InternalRoomId = personalId;
            _users.TryAdd(personalId, user);

            DynamicRoomModel model = _room.GetGameMap().Model;

            if ((bot.X > 0 && bot.Y > 0) && bot.X < model.MapSizeX && bot.Y < model.MapSizeY) {
                user.SetPos(bot.X, bot.Y, bot.Z);
                user.SetRot(bot.Rot, false);
            } else {
                bot.X = model.DoorX;
                bot.Y = model.DoorY;

                user.SetPos(model.DoorX, model.DoorY, model.DoorZ);
                user.SetRot(model.DoorOrientation, false);
            }

            user.BotData = bot;
            user.BotAI = bot.GenerateBotAI(user.VirtualId);

            if (user.IsPet) {
                user.BotAI.Init(bot.BotId, user.VirtualId, _room.RoomId, user, _room);
                user.PetData = pet;
                user.PetData.VirtualId = user.VirtualId;
            } else
                user.BotAI.Init(bot.BotId, user.VirtualId, _room.RoomId, user, _room);

            user.UpdateNeeded = true;

            _room.SendPacket(new UsersComposer(user));

            if (user.IsPet) {
                if (_pets.ContainsKey(user.PetData.PetId))
                    _pets[user.PetData.PetId] = user;
                else
                    _pets.TryAdd(user.PetData.PetId, user);

                PetCount++;
            } else if (user.IsBot) {
                if (_bots.ContainsKey(user.BotData.BotId))
                    _bots[user.BotData.BotId] = user;
                else
                    _bots.TryAdd(user.BotData.Id, user);

                _room.SendPacket(new DanceComposer(user.VirtualId, user.BotData.DanceId));

                if (user.BotData.EffectId > 0)
                    user.ApplyEffect(user.BotData.EffectId);
            }

            return user;
        }

        public void RemoveBot(int virtualId, bool kicked)
        {
            RoomUser user = GetRoomUserByVirtualId(virtualId);
            if (user == null || !user.IsBot)
                return;

            if (user.IsPet) {
                _pets.TryRemove(user.PetData.PetId, out RoomUser pet);
                PetCount--;
            } else {
                _bots.TryRemove(user.BotData.Id, out RoomUser bot);
            }

            user.BotAI.OnSelfLeaveRoom(kicked);

            _room.SendPacket(new UserRemoveComposer(user.VirtualId));

            if (_users != null)
                _users.TryRemove(user.InternalRoomId, out RoomUser toRemove);

            OnRemove(user);
        }

        public RoomUser GetUserForSquare(int x, int y)
        {
            return _room.GetGameMap().GetRoomUsers(new Point(x, y)).FirstOrDefault();
        }

        public bool AddAvatarToRoom(GameClient session)
        {
            if (_room == null)
                return false;

            if (session?.GetHabbo().CurrentRoom == null)
                return false;

            // Entry packets can be retriggered while the session is already present in the room.
            // If the cached room user belongs to a stale client/session, remove it and recreate
            // the avatar so the new session can move normally.
            RoomUser existingUser = GetRoomUserByHabbo(session.GetHabbo().Id);
            if (existingUser != null) {
                if (ReferenceEquals(existingUser.GetClient(), session))
                    return true;

                RemoveRoomUser(existingUser);
            }

            RoomUser user = new(session.GetHabbo().Id, _room.RoomId, _primaryPrivateUserId++, _room);

            if (user == null || user.GetClient() == null)
                return false;

            user.UserId = session.GetHabbo().Id;

            session.GetHabbo().TentId = 0;

            int personalId = _secondaryPrivateUserId++;
            user.InternalRoomId = personalId;

            session.GetHabbo().CurrentRoomId = _room.RoomId;
            if (!_users.TryAdd(personalId, user))
                return false;

            DynamicRoomModel model = _room.GetGameMap().Model;
            if (model == null)
                return false;

            if (!_room.PetMorphsAllowed && session.GetHabbo().PetId != 0)
                session.GetHabbo().PetId = 0;

            if (!session.GetHabbo().IsTeleporting && !session.GetHabbo().IsHopping) {
                if (!model.DoorIsValid()) {
                    Point square = _room.GetGameMap().GetRandomWalkableSquare();
                    model.DoorX = square.X;
                    model.DoorY = square.Y;
                    model.DoorZ = _room.GetGameMap().GetHeightForSquareFromData(square);
                }

                user.SetPos(model.DoorX, model.DoorY, model.DoorZ);
                user.SetRot(model.DoorOrientation, false);
            } else if (!user.IsBot && (user.GetClient().GetHabbo().IsTeleporting || user.GetClient().GetHabbo().IsHopping)) {
                Item item = null;
                bool handledArrowArrival = false;
                if (session.GetHabbo().IsTeleporting) {
                    if (Plus.HabboHotel.Items.Interactor.InteractorArrowTeleport.TryTakeArrivalData(session.GetHabbo().Id, _room.RoomId, out int targetItemId, out int bodyRotation, out int headRotation)) {
                        item = _room.GetRoomItemHandler().GetItem(targetItemId);
                        if (item != null) {
                            item.ExtraData = "2";
                            item.UpdateState(false, true);
                            user.SetPos(item.GetX, item.GetY, item.GetZ);
                            user.SetRot(bodyRotation, false);
                            user.RotHead = headRotation;
                            item.InteractingUser2 = session.GetHabbo().Id;
                            item.ExtraData = "0";
                            item.UpdateState(false, true);
                            handledArrowArrival = true;
                        }
                    }

                    if (item == null)
                        item = _room.GetRoomItemHandler().GetItem(session.GetHabbo().TeleportId);
                } else if (session.GetHabbo().IsHopping)
                    item = _room.GetRoomItemHandler().GetItem(session.GetHabbo().HopperId);

                if (item != null) {
                    if (session.GetHabbo().IsTeleporting) {
                        if (!handledArrowArrival) {
                            item.ExtraData = "2";
                            item.UpdateState(false, true);
                            user.SetPos(item.GetX, item.GetY, item.GetZ);
                            user.SetRot(item.Rotation, false);
                            item.InteractingUser2 = session.GetHabbo().Id;
                            item.ExtraData = "0";
                            item.UpdateState(false, true);
                        }

                        session.GetHabbo().IsTeleporting = false;
                        session.GetHabbo().TeleportId = 0;
                        session.GetHabbo().TeleportingRoomId = 0;
                    } else if (session.GetHabbo().IsHopping) {
                        item.ExtraData = "1";
                        item.UpdateState(false, true);
                        user.SetPos(item.GetX, item.GetY, item.GetZ);
                        user.SetRot(item.Rotation, false);
                        user.AllowOverride = false;
                        item.InteractingUser2 = session.GetHabbo().Id;
                        item.ExtraData = "2";
                        item.UpdateState(false, true);
                    }
                } else {
                    user.SetPos(model.DoorX, model.DoorY, model.DoorZ - 1);
                    user.SetRot(model.DoorOrientation, false);
                }
            }

            _room.SendPacket(new UsersComposer(user));

            if (_room.CheckRights(session, true)) {
                user.SetStatus("flatctrl", "useradmin");
                session.SendPacket(new YouAreOwnerComposer());
                session.SendPacket(new YouAreControllerComposer(5));
            } else if (_room.CheckRights(session, false) && _room.Group == null) {
                user.SetStatus("flatctrl", "1");
                session.SendPacket(new YouAreControllerComposer(1));
            } else if (_room.Group != null && _room.CheckRights(session, false, true)) {
                user.SetStatus("flatctrl", "3");
                session.SendPacket(new YouAreControllerComposer(3));
            } else
                session.SendPacket(new YouAreNotControllerComposer());

            user.UpdateNeeded = true;

            RpEffectService.Refresh(session.GetHabbo());

            foreach (RoomUser bot in _bots.Values.ToList())
            {
                if (bot == null || bot.BotAI == null)
                    continue;

                bot.BotAI.OnUserEnterRoom(user);
            }

            return true;
        }

        public void RemoveUserFromRoom(GameClient session, bool notifyUser, bool notifyKick = false)
        {
            try
            {
                if (_room == null)
                    return;

                if (session == null || session.GetHabbo() == null)
                    return;

                if (notifyKick)
                    session.SendPacket(new GenericErrorComposer(4008));

                if (notifyUser)
                    session.SendPacket(new CloseConnectionComposer());

                if (session.GetHabbo().TentId > 0)
                    session.GetHabbo().TentId = 0;

                if (!RoomFactory.TryGetData(_room.RoomId, out RoomData data)) {
                    return;
                }

                data.UsersNow--;

                RoomUser user = GetRoomUserByHabbo(session.GetHabbo().Id);
                if (user != null) {
                    if (user.RidingHorse) {
                        user.RidingHorse = false;
                        RoomUser userRiding = GetRoomUserByVirtualId(user.HorseId);
                        if (userRiding != null) {
                            userRiding.RidingHorse = false;
                            userRiding.HorseId = 0;
                        }
                    }

                    if (user.Team != Team.None) {
                        TeamManager team = _room.GetTeamManagerForFreeze();
                        if (team != null) {
                            team.OnUserLeave(user);

                            user.Team = Team.None;

                            if (user.GetClient().GetHabbo().Effects().CurrentEffect != 0)
                                user.GetClient().GetHabbo().Effects().ApplyEffect(0);
                        }
                    }

                    // When notifyUser is false, the user is transitioning to another room
                    // (not disconnecting). Exclude them from the UserRemove broadcast so
                    // the Nitro client doesn't receive a self-removal that kills the new
                    // room session. Other users in the room still see the removal.
                    RemoveRoomUser(user, notifyUser ? null : session);

                    if (user.CurrentItemEffect != ItemEffectType.None) {
                        if (session.GetHabbo().Effects() != null)
                            session.GetHabbo().Effects().CurrentEffect = -1;
                    }

                    if (user.IsTrading) {
                        if (_room.GetTrading().TryGetTrade(user.TradeId, out Trade trade))
                            trade.EndTrade(user.TradeId);
                    }

                    session.GetHabbo().CurrentRoomId = 0;

                    if (session.GetHabbo().GetMessenger() != null)
                        session.GetHabbo().GetMessenger().OnStatusChanged(true);

                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery("UPDATE user_roomvisits SET exit_timestamp = '" + DateTimeOffset.UtcNow.ToUnixTimeSeconds() + "' WHERE room_id = '" + _room.RoomId + "' AND user_id = '" + session.GetHabbo().Id + "' ORDER BY exit_timestamp DESC LIMIT 1");
                        dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '" + _room.UsersNow + "' WHERE `id` = '" + _room.RoomId + "' LIMIT 1");
                    }

                    user.Dispose();
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        private void OnRemove(RoomUser user)
        {
            try
            {
                GameClient session = user.GetClient();
                if (session == null)
                    return;

                PlusEnvironment.GetGame().GetBotManager().HandleUserRoomLeave(session, _room);

                List<RoomUser> bots = new();

                try
                {
                    foreach (RoomUser roomUser in GetUserList().ToList())
                    {
                        if (roomUser == null)
                            continue;

                        if (roomUser.IsBot && !roomUser.IsPet) {
                            if (!bots.Contains(roomUser))
                                bots.Add(roomUser);
                        }
                    }
                }
                catch
                {
                }

                List<RoomUser> petsToRemove = new();
                foreach (RoomUser bot in bots.ToList())
                {
                    if (bot == null || bot.BotAI == null)
                        continue;

                    bot.BotAI.OnUserLeaveRoom(session);

                    if (bot.IsPet && bot.PetData.OwnerId == user.UserId && !_room.CheckRights(session, true)) {
                        if (!petsToRemove.Contains(bot))
                            petsToRemove.Add(bot);
                    }
                }

                foreach (RoomUser toRemove in petsToRemove.ToList())
                {
                    if (toRemove == null)
                        continue;

                    if (user.GetClient() == null || user.GetClient().GetHabbo() == null || user.GetClient().GetHabbo().GetInventoryComponent() == null)
                        continue;

                    if (user.GetClient().GetHabbo().GetInventoryComponent().TryAddPet(toRemove.PetData)) {
                        toRemove.PetData.RoomId = 0;
                        toRemove.PetData.PlacedInRoom = false;

                        RemoveBot(toRemove.VirtualId, false);
                    }
                }

                _room.GetGameMap().RemoveUserFromMap(user, new Point(user.X, user.Y));
            }
            catch (Exception e)
            {
                ExceptionLogger.LogCriticalException(e);
            }
        }

        private void RemoveRoomUser(RoomUser user, GameClient excludeFromBroadcast = null)
        {
            if (user.SetStep)
                _room.GetGameMap().GameMap[user.SetX, user.SetY] = user.SqState;
            else
                _room.GetGameMap().GameMap[user.X, user.Y] = user.SqState;

            _room.GetGameMap().RemoveUserFromMap(user, new Point(user.X, user.Y));

            if (excludeFromBroadcast != null)
                _room.SendPacketExcept(new UserRemoveComposer(user.VirtualId), excludeFromBroadcast);
            else
                _room.SendPacket(new UserRemoveComposer(user.VirtualId));

            if (_users.TryRemove(user.InternalRoomId, out RoomUser toRemove)) {
            }

            user.InternalRoomId = -1;
            OnRemove(user);
        }

        public bool TryGetPet(int petId, out RoomUser pet)
        {
            return _pets.TryGetValue(petId, out pet);
        }

        public bool TryGetBot(int botId, out RoomUser bot)
        {
            return _bots.TryGetValue(botId, out bot);
        }

        public RoomUser GetBotByName(string name)
        {
            bool foundBot = _bots.Any(x => x.Value.BotData != null && x.Value.BotData.Name.ToLower() == name.ToLower());
            if (foundBot) {
                int id = _bots.FirstOrDefault(x => x.Value.BotData != null && x.Value.BotData.Name.ToLower() == name.ToLower()).Value.BotData.Id;

                return _bots[id];
            }

            return null;
        }

        public void UpdateUserCount(int count)
        {
            UserCount = count;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '" + count + "' WHERE `id` = '" + _room.RoomId + "' LIMIT 1");
            }
        }

        public RoomUser GetRoomUserByVirtualId(int virtualId)
        {
            if (!_users.TryGetValue(virtualId, out RoomUser user))
                return null;
            return user;
        }

        public RoomUser GetRoomUserByHabbo(int id)
        {
            return GetUserList().FirstOrDefault(x => x != null && x.GetClient() != null && x.GetClient().GetHabbo() != null && x.GetClient().GetHabbo().Id == id);
        }

        public List<RoomUser> GetRoomUsers()
        {
            return GetUserList().Where(x => !x.IsBot).ToList();
        }

        public List<RoomUser> GetRoomUserByRank(int minRank)
        {
            var returnList = new List<RoomUser>();
            foreach (RoomUser user in GetUserList().ToList())
            {
                if (user == null)
                    continue;

                if (!user.IsBot && user.GetClient() != null && user.GetClient().GetHabbo() != null && user.GetClient().GetHabbo().Rank >= minRank)
                    returnList.Add(user);
            }

            return returnList;
        }

        public RoomUser GetRoomUserByHabbo(string pName)
        {
            return GetUserList().FirstOrDefault(x => x != null && x.GetClient() != null && x.GetClient().GetHabbo() != null && x.GetClient().GetHabbo().Username.Equals(pName, StringComparison.OrdinalIgnoreCase));
        }

        public void UpdatePets()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                foreach (Pet pet in GetPets().ToList())
                {
                    if (pet == null)
                        continue;

                    if (pet.DbState == PetDatabaseUpdateState.NeedsInsert) {
                        dbClient.SetQuery("INSERT INTO `bots` (`id`,`user_id`,`room_id`,`name`,`x`,`y`,`z`) VALUES ('" + pet.PetId + "','" + pet.OwnerId + "','" + pet.RoomId + "',@name,'0','0','0')");
                        dbClient.AddParameter("name", pet.Name);
                        dbClient.RunQuery();

                        dbClient.SetQuery("INSERT INTO `bots_petdata` (`type`,`race`,`color`,`experience`,`energy`,`createstamp`,`nutrition`,`respect`) VALUES ('" + pet.Type + "',@race,@color,'0','100','" + pet.CreationStamp + "','0','0')");
                        dbClient.AddParameter(pet.PetId + "race", pet.Race);
                        dbClient.AddParameter(pet.PetId + "color", pet.Color);
                        dbClient.RunQuery();
                    } else if (pet.DbState == PetDatabaseUpdateState.NeedsUpdate) {
                        //Surely this can be *99 better? // TODO
                        RoomUser user = GetRoomUserByVirtualId(pet.VirtualId);

                        dbClient.RunQuery("UPDATE `bots` SET room_id = " + pet.RoomId + ", x = " + (user != null ? user.X : 0) + ", Y = " + (user != null ? user.Y : 0) + ", Z = " + (user != null ? user.Z : 0) + " WHERE `id` = '" + pet.PetId + "' LIMIT 1");
                        dbClient.RunQuery("UPDATE `bots_petdata` SET `experience` = '" + pet.Experience + "', `energy` = '" + pet.Energy + "', `nutrition` = '" + pet.Nutrition + "', `respect` = '" + pet.Respect + "' WHERE `id` = '" + pet.PetId + "' LIMIT 1");
                    }

                    pet.DbState = PetDatabaseUpdateState.Updated;
                }
            }
        }

        private void UpdateBots()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                foreach (RoomUser user in GetRoomUsers().ToList())
                {
                    if (user == null || !user.IsBot)
                        continue;

                    if (user.IsBot) {
                        dbClient.SetQuery("UPDATE bots SET x=@x, y=@y, z=@z, name=@name, look=@look, rotation=@rotation WHERE id=@id LIMIT 1;");
                        dbClient.AddParameter("name", user.BotData.Name);
                        dbClient.AddParameter("look", user.BotData.Look);
                        dbClient.AddParameter("rotation", user.BotData.Rot);
                        dbClient.AddParameter("x", user.X);
                        dbClient.AddParameter("y", user.Y);
                        dbClient.AddParameter("z", user.Z);
                        dbClient.AddParameter("id", user.BotData.BotId);
                        dbClient.RunQuery();
                    }
                }
            }
        }

        public List<Pet> GetPets()
        {
            List<Pet> pets = new();
            foreach (RoomUser user in _pets.Values.ToList())
            {
                if (user == null || !user.IsPet)
                    continue;

                pets.Add(user.PetData);
            }

            return pets;
        }

        public void SerializeStatusUpdates()
        {
            List<RoomUser> users = new();
            ICollection<RoomUser> roomUsers = GetUserList();

            if (roomUsers == null)
                return;

            foreach (RoomUser user in roomUsers.ToList())
            {
                if (user == null || !user.UpdateNeeded || users.Contains(user))
                    continue;

                user.UpdateNeeded = false;
                users.Add(user);
            }

            if (users.Count > 0)
                _room.SendPacket(new UserUpdateComposer(users));
        }

        public void UpdateUserStatusses()
        {
            foreach (RoomUser user in GetUserList().ToList())
            {
                if (user == null)
                    continue;

                UpdateUserStatus(user, false);
            }
        }

        private bool IsValid(RoomUser user)
        {
            if (user == null)
                return false;
            if (user.IsBot)
                return true;
            if (user.GetClient() == null)
                return false;
            if (user.GetClient().GetHabbo() == null)
                return false;
            if (user.GetClient().GetHabbo().CurrentRoomId != _room.RoomId)
                return false;
            return true;
        }

        public void OnCycle()
        {
            int userCounter = 0;

            try
            {
                List<RoomUser> ToRemove = new();

                foreach (RoomUser user in GetUserList().ToList())
                {
                    if (user == null)
                        continue;

                    if (!IsValid(user)) {
                        if (user.GetClient() != null)
                            RemoveUserFromRoom(user.GetClient(), false);
                        else
                            RemoveRoomUser(user);
                    }

                    if (user.NeedsAutoKick && !ToRemove.Contains(user)) {
                        ToRemove.Add(user);
                        continue;
                    }

                    bool updated = false;
                    user.IdleTime++;
                    user.HandleSpamTicks();
                    if (!user.IsBot && !user.IsAsleep && user.IdleTime >= 600) {
                        user.IsAsleep = true;
                        _room.SendPacket(new SleepComposer(user.VirtualId, true));

                        // Going idle ends any active work shift.
                        if (user.GetClient()?.GetHabbo() != null)
                            PlusEnvironment.GetGame().GetShiftManager().InterruptShift(user.GetClient().GetHabbo(), _room);
                    }

                    if (user.CarryItemId > 0) {
                        user.CarryTimer--;
                        if (user.CarryTimer <= 0)
                            user.CarryItem(0);
                    }

                    if (_room.GotFreeze())
                        _room.GetFreeze().CycleUser(user);

                    bool invalidStep = false;

                    if (user.IsRolling) {
                        if (user.RollerDelay <= 0) {
                            UpdateUserStatus(user, false);
                            user.IsRolling = false;
                        } else
                            user.RollerDelay--;
                    }

                    if (user.SetStep) {
                        if (_room.GetGameMap().IsValidStep2(user, new Vector2D(user.X, user.Y), new Vector2D(user.SetX, user.SetY), (user.GoalX == user.SetX && user.GoalY == user.SetY), user.AllowOverride)) {
                            if (!user.RidingHorse)
                                _room.GetGameMap().UpdateUserMovement(new Point(user.Coordinate.X, user.Coordinate.Y), new Point(user.SetX, user.SetY), user);

                            List<Item> items = _room.GetGameMap().GetCoordinatedItems(new Point(user.X, user.Y));
                            foreach (Item item in items.ToList())
                            {
                                item.UserWalksOffFurni(user);
                            }

                            if (!user.IsBot) {
                                user.X = user.SetX;
                                user.Y = user.SetY;
                                user.Z = user.SetZ;
                            } else if (user.IsBot && !user.RidingHorse) {
                                user.X = user.SetX;
                                user.Y = user.SetY;
                                user.Z = user.SetZ;
                            }

                            if (!user.IsBot && user.RidingHorse) {
                                RoomUser horse = GetRoomUserByVirtualId(user.HorseId);
                                if (horse != null) {
                                    horse.X = user.SetX;
                                    horse.Y = user.SetY;
                                }
                            }

                            List<Item> Items = _room.GetGameMap().GetCoordinatedItems(new Point(user.X, user.Y));
                            foreach (Item item in Items.ToList())
                            {
                                item.UserWalksOnFurni(user);

                                if (!user.IsBot && item.GetBaseItem()?.InteractionType == InteractionType.Arrow)
                                    Plus.HabboHotel.Items.Interactor.InteractorArrowTeleport.BeginDelayedTeleport(user, item);
                            }

                            UpdateUserStatus(user, true);
                        } else
                            invalidStep = true;

                        user.SetStep = false;
                    }

                    if (user.PathRecalcNeeded) {
                        if (user.Path.Count > 1)
                            user.Path.Clear();

                        user.Path = PathFinder.FindPath(user, _room.GetGameMap().DiagonalEnabled, _room.GetGameMap(), new Vector2D(user.X, user.Y), new Vector2D(user.GoalX, user.GoalY));

                        if (user.Path.Count > 1) {
                            user.PathStep = 1;
                            user.IsWalking = true;
                            user.PathRecalcNeeded = false;
                        } else {
                            user.PathRecalcNeeded = false;
                            if (user.Path.Count > 1)
                                user.Path.Clear();
                        }
                    }

                    if (user.IsWalking && !user.Freezed) {
                        if (invalidStep || (user.PathStep >= user.Path.Count) || (user.GoalX == user.X && user.GoalY == user.Y)) //No path found, or reached goal (:
                        {
                            user.IsWalking = false;
                            user.RemoveStatus("mv");

                            if (user.Statusses.ContainsKey("sign"))
                                user.RemoveStatus("sign");

                            if (user.IsBot && user.BotData.TargetUser > 0) {
                                if (user.CarryItemId > 0) {
                                    RoomUser target = _room.GetRoomUserManager().GetRoomUserByHabbo(user.BotData.TargetUser);

                                    if (target != null && Gamemap.TilesTouching(user.X, user.Y, target.X, target.Y)) {
                                        user.SetRot(Rotation.Calculate(user.X, user.Y, target.X, target.Y), false);
                                        target.SetRot(Rotation.Calculate(target.X, target.Y, user.X, user.Y), false);
                                        target.CarryItem(user.CarryItemId);
                                    }
                                }

                                user.CarryItem(0);
                                user.BotData.TargetUser = 0;
                            }

                            if (user.RidingHorse && user.IsPet == false && !user.IsBot) {
                                RoomUser mascotaVinculada = GetRoomUserByVirtualId(user.HorseId);
                                if (mascotaVinculada != null) {
                                    mascotaVinculada.IsWalking = false;
                                    mascotaVinculada.RemoveStatus("mv");
                                    mascotaVinculada.UpdateNeeded = true;
                                }
                            }
                        } else {
                            Vector2D nextStep = user.Path[(user.Path.Count - user.PathStep) - 1];
                            user.PathStep++;

                            if (user.FastWalking && user.PathStep < user.Path.Count) {
                                int s2 = (user.Path.Count - user.PathStep) - 1;
                                nextStep = user.Path[s2];
                                user.PathStep++;
                            }

                            if (user.SuperFastWalking && user.PathStep < user.Path.Count) {
                                int s2 = (user.Path.Count - user.PathStep) - 1;
                                nextStep = user.Path[s2];
                                user.PathStep++;
                                user.PathStep++;
                            }

                            int nextX = nextStep.X;
                            int nextY = nextStep.Y;
                            user.RemoveStatus("mv");

                            if (_room.GetGameMap().IsValidStep2(user, new Vector2D(user.X, user.Y), new Vector2D(nextX, nextY), (user.GoalX == nextX && user.GoalY == nextY), user.AllowOverride)) {
                                double nextZ = _room.GetGameMap().SqAbsoluteHeight(nextX, nextY);

                                if (!user.IsBot) {
                                    if (user.IsSitting) {
                                        user.Statusses.Remove("sit");
                                        user.Z += 0.35;
                                        user.IsSitting = false;
                                        user.UpdateNeeded = true;
                                    } else if (user.IsLying) {
                                        user.Statusses.Remove("sit");
                                        user.Z += 0.35;
                                        user.IsLying = false;
                                        user.UpdateNeeded = true;
                                    }
                                }

                                if (!user.IsBot) {
                                    user.Statusses.Remove("lay");
                                    user.Statusses.Remove("sit");
                                }

                                if (!user.IsBot && !user.IsPet && user.GetClient() != null) {
                                    if (user.GetClient().GetHabbo().IsHopping) {
                                        user.GetClient().GetHabbo().IsHopping = false;
                                        user.GetClient().GetHabbo().HopperId = 0;
                                    }
                                }

                                if (!user.IsBot && user.RidingHorse && user.IsPet == false) {
                                    RoomUser horse = GetRoomUserByVirtualId(user.HorseId);
                                    horse?.SetStatus("mv", nextX + "," + nextY + "," + TextHandling.GetString(nextZ));

                                    user.SetStatus("mv", +nextX + "," + nextY + "," + TextHandling.GetString(nextZ + 1));

                                    user.UpdateNeeded = true;
                                    horse.UpdateNeeded = true;
                                } else
                                    user.SetStatus("mv", nextX + "," + nextY + "," + TextHandling.GetString(nextZ));

                                int newRot = Rotation.Calculate(user.X, user.Y, nextX, nextY, user.MoonwalkEnabled);

                                user.RotBody = newRot;
                                user.RotHead = newRot;

                                user.SetStep = true;
                                user.SetX = nextX;
                                user.SetY = nextY;
                                user.SetZ = nextZ;
                                UpdateUserEffect(user, user.SetX, user.SetY);

                                updated = true;

                                if (user.RidingHorse && user.IsPet == false && !user.IsBot) {
                                    RoomUser horse = GetRoomUserByVirtualId(user.HorseId);
                                    if (horse != null) {
                                        horse.RotBody = newRot;
                                        horse.RotHead = newRot;

                                        horse.SetStep = true;
                                        horse.SetX = nextX;
                                        horse.SetY = nextY;
                                        horse.SetZ = nextZ;
                                    }
                                }

                                _room.GetGameMap().GameMap[user.X, user.Y] = user.SqState; // REstore the old one
                                user.SqState = _room.GetGameMap().GameMap[user.SetX, user.SetY]; //Backup the new one

                                if (_room.RoomBlockingEnabled == 0) {
                                    RoomUser users = _room.GetRoomUserManager().GetUserForSquare(nextX, nextY);
                                    if (users != null)
                                        _room.GetGameMap().GameMap[nextX, nextY] = 0;
                                } else
                                    _room.GetGameMap().GameMap[nextX, nextY] = 1;
                            }
                        }

                        if (!user.RidingHorse)
                            user.UpdateNeeded = true;
                    } else {
                        if (user.Statusses.ContainsKey("mv")) {
                            user.RemoveStatus("mv");
                            user.UpdateNeeded = true;

                            if (user.RidingHorse) {
                                RoomUser horse = GetRoomUserByVirtualId(user.HorseId);
                                if (horse != null) {
                                    horse.RemoveStatus("mv");
                                    horse.UpdateNeeded = true;
                                }
                            }
                        }
                    }

                    if (user.RidingHorse)
                        user.ApplyEffect(77);

                    if (user.IsBot && user.BotAI != null)
                        user.BotAI.OnTimerTick();
                    else
                        userCounter++;

                    if (!updated) {
                        UpdateUserEffect(user, user.X, user.Y);
                    }
                }

                foreach (RoomUser toRemove in ToRemove.ToList())
                {
                    GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(toRemove.HabboId);
                    if (client != null) {
                        RemoveUserFromRoom(client, true);
                    } else
                        RemoveRoomUser(toRemove);
                }

                if (UserCount != userCounter)
                    UpdateUserCount(userCounter);
            }
            catch (Exception e)
            {
                ExceptionLogger.LogCriticalException(e);
            }
        }

        public void UpdateUserStatus(RoomUser user, bool cycleGameItems)
        {
            if (user == null)
                return;

            try
            {
                bool isBot = user.IsBot;
                if (isBot)
                    cycleGameItems = false;

                if (PlusEnvironment.GetUnixTimestamp() > PlusEnvironment.GetUnixTimestamp() + user.SignTime) {
                    if (user.Statusses.ContainsKey("sign")) {
                        user.Statusses.Remove("sign");
                        user.UpdateNeeded = true;
                    }
                }

                if ((user.Statusses.ContainsKey("lay") && !user.IsLying) || (user.Statusses.ContainsKey("sit") && !user.IsSitting)) {
                    if (user.Statusses.ContainsKey("lay"))
                        user.Statusses.Remove("lay");
                    if (user.Statusses.ContainsKey("sit"))
                        user.Statusses.Remove("sit");
                    user.UpdateNeeded = true;
                } else if (user.IsLying || user.IsSitting)
                    return;

                double newZ;
                List<Item> itemsOnSquare = _room.GetGameMap().GetAllRoomItemForSquare(user.X, user.Y);
                if (itemsOnSquare != null || itemsOnSquare.Count != 0) {
                    if (user.RidingHorse && user.IsPet == false)
                        newZ = _room.GetGameMap().SqAbsoluteHeight(user.X, user.Y, itemsOnSquare.ToList()) + 1;
                    else
                        newZ = _room.GetGameMap().SqAbsoluteHeight(user.X, user.Y, itemsOnSquare.ToList());
                } else {
                    newZ = 1;
                }

                if (newZ != user.Z) {
                    user.Z = newZ;
                    user.UpdateNeeded = true;
                }

                DynamicRoomModel model = _room.GetGameMap().Model;
                if (model.SqState[user.X, user.Y] == SquareState.Seat) {
                    if (!user.Statusses.ContainsKey("sit"))
                        user.Statusses.Add("sit", "1.0");
                    user.Z = model.SqFloorHeight[user.X, user.Y];
                    user.RotHead = model.SqSeatRot[user.X, user.Y];
                    user.RotBody = model.SqSeatRot[user.X, user.Y];

                    user.UpdateNeeded = true;
                }


                if (itemsOnSquare.Count == 0)
                    user.LastItem = null;


                foreach (Item item in itemsOnSquare.ToList())
                {
                    if (item == null)
                        continue;

                    if (item.GetBaseItem().IsSeat) {
                        if (!user.Statusses.ContainsKey("sit")) {
                            if (!user.Statusses.ContainsKey("sit"))
                                user.Statusses.Add("sit", TextHandling.GetString(item.GetBaseItem().Height));
                        }

                        user.Z = item.GetZ;
                        user.RotHead = item.Rotation;
                        user.RotBody = item.Rotation;
                        user.UpdateNeeded = true;
                    }

                    switch (item.GetBaseItem().InteractionType)
                    {
                        #region Beds & Tents

                        case InteractionType.Bed:
                        case InteractionType.TentSmall:
                        {
                            if (!user.Statusses.ContainsKey("lay"))
                                user.Statusses.Add("lay", TextHandling.GetString(item.GetBaseItem().Height) + " null");

                            user.Z = item.GetZ;
                            user.RotHead = item.Rotation;
                            user.RotBody = item.Rotation;

                            user.UpdateNeeded = true;
                            break;
                        }

                        #endregion

                        #region Banzai Gates

                        case InteractionType.BanzaiGateGreen:
                        case InteractionType.BanzaiGateBlue:
                        case InteractionType.BanzaiGateRed:
                        case InteractionType.BanzaiGateYellow:
                        {
                            if (cycleGameItems) {
                                int effectId = Convert.ToInt32(item.Team + 32);
                                TeamManager t = user.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForBanzai();

                                if (user.Team == Team.None) {
                                    if (t.CanEnterOnTeam(item.Team)) {
                                        if (user.Team != Team.None)
                                            t.OnUserLeave(user);
                                        user.Team = item.Team;

                                        t.AddUser(user);

                                        if (user.GetClient().GetHabbo().Effects().CurrentEffect != effectId)
                                            user.GetClient().GetHabbo().Effects().ApplyEffect(effectId);
                                    }
                                } else if (user.Team != Team.None && user.Team != item.Team) {
                                    t.OnUserLeave(user);
                                    user.Team = Team.None;
                                    user.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                } else {
                                    //usersOnTeam--;
                                    t.OnUserLeave(user);
                                    if (user.GetClient().GetHabbo().Effects().CurrentEffect == effectId)
                                        user.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                    user.Team = Team.None;
                                }
                                //Item.ExtraData = usersOnTeam.ToString();
                                //Item.UpdateState(false, true);                                
                            }

                            break;
                        }

                        #endregion

                        #region Freeze Gates

                        case InteractionType.FreezeYellowGate:
                        case InteractionType.FreezeRedGate:
                        case InteractionType.FreezeGreenGate:
                        case InteractionType.FreezeBlueGate:
                        {
                            if (cycleGameItems) {
                                int effectId = Convert.ToInt32(item.Team + 39);
                                TeamManager t = user.GetClient().GetHabbo().CurrentRoom.GetTeamManagerForFreeze();

                                if (user.Team == Team.None) {
                                    if (t.CanEnterOnTeam(item.Team)) {
                                        if (user.Team != Team.None)
                                            t.OnUserLeave(user);
                                        user.Team = item.Team;
                                        t.AddUser(user);

                                        if (user.GetClient().GetHabbo().Effects().CurrentEffect != effectId)
                                            user.GetClient().GetHabbo().Effects().ApplyEffect(effectId);
                                    }
                                } else if (user.Team != Team.None && user.Team != item.Team) {
                                    t.OnUserLeave(user);
                                    user.Team = Team.None;
                                    user.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                } else {
                                    //usersOnTeam--;
                                    t.OnUserLeave(user);
                                    if (user.GetClient().GetHabbo().Effects().CurrentEffect == effectId)
                                        user.GetClient().GetHabbo().Effects().ApplyEffect(0);
                                    user.Team = Team.None;
                                }
                                //Item.ExtraData = usersOnTeam.ToString();
                                //Item.UpdateState(false, true);                                
                            }

                            break;
                        }

                        #endregion

                        #region Banzai Teles

                        case InteractionType.BanzaiTele:
                        {
                            if (user.Statusses.ContainsKey("mv"))
                                _room.GetGameItemHandler().OnTeleportRoomUserEnter(user, item);
                            break;
                        }

                        #endregion

                        #region Football Gate

                        #endregion

                        #region Effects

                        case InteractionType.Effect:
                        {
                            if (user == null)
                                return;

                            if (!user.IsBot) {
                                if (item == null || item.GetBaseItem() == null || user.GetClient() == null || user.GetClient().GetHabbo() == null || user.GetClient().GetHabbo().Effects() == null)
                                    return;

                                if (item.GetBaseItem().EffectId == 0 && user.GetClient().GetHabbo().Effects().CurrentEffect == 0)
                                    return;

                                user.GetClient().GetHabbo().Effects().ApplyEffect(item.GetBaseItem().EffectId);
                                item.ExtraData = "1";
                                item.UpdateState(false, true);
                                item.RequestUpdate(2, true);
                            }

                            break;
                        }

                        #endregion

                    }
                }

                if (user.IsSitting && user.TeleportEnabled) {
                    user.Z -= 0.35;
                    user.UpdateNeeded = true;
                }

                if (cycleGameItems) {
                    if (_room.GotSoccer())
                        _room.GetSoccer().OnUserWalk(user);

                    if (_room.GotBanzai())
                        _room.GetBanzai().OnUserWalk(user);

                    if (_room.GotFreeze())
                        _room.GetFreeze().OnUserWalk(user);
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogException(e);
            }
        }

        private void UpdateUserEffect(RoomUser user, int x, int y)
        {
            if (user == null || user.IsBot || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            try
            {
                byte newCurrentUserItemEffect = _room.GetGameMap().EffectMap[x, y];
                if (newCurrentUserItemEffect > 0) {
                    if (user.GetClient().GetHabbo().Effects().CurrentEffect == 0)
                        user.CurrentItemEffect = ItemEffectType.None;

                    ItemEffectType type = ByteToItemEffectEnum.Parse(newCurrentUserItemEffect);
                    if (type != user.CurrentItemEffect) {
                        switch (type)
                        {
                            case ItemEffectType.IceSkates:
                            {
                                user.GetClient().GetHabbo().Effects().ApplyEffect(user.GetClient().GetHabbo().Gender == "M" ? 38 : 39);
                                user.CurrentItemEffect = ItemEffectType.IceSkates;
                                break;
                            }

                            case ItemEffectType.NormalSkates:
                            {
                                user.GetClient().GetHabbo().Effects().ApplyEffect(user.GetClient().GetHabbo().Gender == "M" ? 55 : 56);
                                user.CurrentItemEffect = type;
                                break;
                            }
                            case ItemEffectType.Swim:
                            {
                                user.GetClient().GetHabbo().Effects().ApplyEffect(29);
                                user.CurrentItemEffect = type;
                                break;
                            }
                            case ItemEffectType.SwimLow:
                            {
                                user.GetClient().GetHabbo().Effects().ApplyEffect(30);
                                user.CurrentItemEffect = type;
                                break;
                            }
                            case ItemEffectType.SwimHalloween:
                            {
                                user.GetClient().GetHabbo().Effects().ApplyEffect(37);
                                user.CurrentItemEffect = type;
                                break;
                            }

                            case ItemEffectType.None:
                            {
                                user.GetClient().GetHabbo().Effects().ApplyEffect(-1);
                                user.CurrentItemEffect = type;
                                break;
                            }
                        }
                    }
                } else if (user.CurrentItemEffect != ItemEffectType.None && newCurrentUserItemEffect == 0) {
                    user.GetClient().GetHabbo().Effects().ApplyEffect(-1);
                    user.CurrentItemEffect = ItemEffectType.None;
                }
            }
            catch
            {
            }
        }

        public int PetCount { get; private set; }

        public ICollection<RoomUser> GetUserList()
        {
            return _users.Values;
        }

        public void Dispose()
        {
            UpdatePets();
            UpdateBots();

            _room.UsersNow = 0;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '0' WHERE `id` = '" + _room.Id + "' LIMIT 1");
            }

            _users.Clear();
            _pets.Clear();
            _bots.Clear();

            UserCount = 0;
            PetCount = 0;

            _users = null;
            _pets = null;
            _bots = null;
            _room = null;
        }
    }
}
