using Plus.Communication.Packets.Outgoing.Handshake;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Core;
using Plus.Database.EF;
using Microsoft.EntityFrameworkCore;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Roleplay.TargetLock;
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
using Plus.Communication.Packets.Outgoing.Roleplay;

namespace Plus.HabboHotel.Rooms
{
    public class RoomUserManager
    {
        private Room _room;
        private ConcurrentDictionary<int, RoomUser> _users;
        private ConcurrentDictionary<int, RoomUser> _bots;
        private ConcurrentDictionary<int, RoomUser> _pets;

        private ConcurrentDictionary<int, RoomUser> _usersByHabboId = new();

        // Immutable snapshot of _users, swapped on join/leave. ConcurrentDictionary.Values builds a
        // fresh copy on every read, and the room reads it on every tick AND on every broadcast
        // packet — chat, statuses, effects — so that copy was the single most-allocated object in
        // a busy room. Membership changes are rare by comparison, so pay there instead.
        // Never mutate a published array: callers iterate it without copying.
        private volatile RoomUser[] _userSnapshot = [];

        // how long it takes to animate a tile at walk speed 1.0
        public const int StepIntervalMs = 500;

        public const int MinStepIntervalMs = 50;

        private int _primaryPrivateUserId;
        private int _secondaryPrivateUserId;

        public int UserCount;

        // Per-cycle snapshot of which tiles hold users, shared by every path searched this tick.
        private byte[] _occupancy;
        private long _occupancyTick = -1;

        // The max window for cancel/recalculation
        private const int MaxStepAbortWindowMs = 75;

        // How far off an item's own height a user may be and still count as standing on/in it.
        // Sitting and lying shift a user 0.35 down, so anything tighter would break seats.
        private const double SameLevelTolerance = 0.5;

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
            RebuildUserSnapshot();

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

            if (_users != null) {
                _users.TryRemove(user.InternalRoomId, out RoomUser toRemove);
                RebuildUserSnapshot();
            }

            OnRemove(user);
        }

        public RoomUser GetUserForSquare(int x, int y)
        {
            return _room.GetGameMap().GetFirstUserForSquare(new Point(x, y));
        }

        public bool AddAvatarToRoom(GameClient session)
        {
            if (_room == null)
                return false;

            if (session?.GetHabbo().CurrentRoom == null)
                return false;

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

            RebuildUserSnapshot();

            _usersByHabboId[user.HabboId] = user;

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

            // Carry session state that lives on the habbo rather than the (per-room)
            // room user: walking mode and the locked RP target.
            user.FastWalking = session.GetHabbo().FastWalkingEnabled;
            user.SuperFastWalking = session.GetHabbo().SuperFastWalkingEnabled;

            // Entering mid-tier: tell the room now rather than waiting on the next full cycle,
            // so the first steps after the door already animate at the right pace.
            if (user.WalkSpeed != RoomUser.WalkSpeedNormal)
                BroadcastWalkSpeed(user);

            TargetLockService.RestoreLock(session.GetHabbo());

            foreach (RoomUser bot in _bots.Values.ToList()) {
                if (bot == null || bot.BotAI == null)
                    continue;

                bot.BotAI.OnUserEnterRoom(user);
            }

            return true;
        }

        public ICollection<RoomUser> GetBots()
        {
            return _bots.Values;
        }

        public void RemoveUserFromRoom(GameClient session, bool notifyUser, bool notifyKick = false)
        {
            try {
                if (_room == null)
                    return;

                if (session == null || session.GetHabbo() == null)
                    return;

                PlusEnvironment.GetGame().GetShiftManager().HandleRoomExit(session.GetHabbo(), _room);

                if (notifyKick)
                    session.SendPacket(new GenericErrorComposer(4008));

                // Involuntary exit (kick / room ban): forward to the configured RP home room,
                // falling back to the hotel view when no home room is configured.
                if (notifyUser) {
                    if (!Plus.HabboHotel.Roleplay.Utilities.RpHomeRoomService.SendHome(session))
                        session.SendPacket(new CloseConnectionComposer());
                }

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

                    // Exit-stamp the visit in the write-behind buffer and let the persistence
                    // scheduler mirror users_now — leaving a room no longer touches MySQL on
                    // the packet thread (previously three queries per exit).
                    Plus.Core.Persistence.RoomVisitBuffer.OnExit(session.GetHabbo().Id);

                    user.Dispose();
                }
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }
        }

        private void OnRemove(RoomUser user)
        {
            try {
                GameClient session = user.GetClient();
                if (session == null)
                    return;

                List<RoomUser> bots = new();

                try {
                    foreach (RoomUser roomUser in GetUserList().ToList()) {
                        if (roomUser == null)
                            continue;

                        if (roomUser.IsBot && !roomUser.IsPet) {
                            if (!bots.Contains(roomUser))
                                bots.Add(roomUser);
                        }
                    }
                } catch {
                }

                List<RoomUser> petsToRemove = new();
                foreach (RoomUser bot in bots.ToList()) {
                    if (bot == null || bot.BotAI == null)
                        continue;

                    bot.BotAI.OnUserLeaveRoom(session);

                    if (bot.IsPet && bot.PetData.OwnerId == user.UserId && !_room.CheckRights(session, true)) {
                        if (!petsToRemove.Contains(bot))
                            petsToRemove.Add(bot);
                    }
                }

                foreach (RoomUser toRemove in petsToRemove.ToList()) {
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
            } catch (Exception e) {
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

            RebuildUserSnapshot();

            // Only unmap the habbo id if it still points at this instance — a rejoin may have
            // already replaced the mapping with the fresh room user.
            if (!user.IsBot && _usersByHabboId.TryGetValue(user.HabboId, out RoomUser mapped) && ReferenceEquals(mapped, user))
                _usersByHabboId.TryRemove(user.HabboId, out _);

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
                int id = _bots.FirstOrDefault(x => x.Value.BotData != null && x.Value.BotData.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)).Value.BotData.Id;

                return _bots[id];
            }

            return null;
        }

        public void UpdateUserCount(int count)
        {
            // In-memory only. rooms.users_now is mirrored to MySQL every 60s by the
            // persistence scheduler (and zeroed on unload/shutdown) — the old code wrote to
            // the database every time the head-count changed in any loaded room.
            UserCount = count;
        }

        public RoomUser GetRoomUserByVirtualId(int virtualId)
        {
            if (!_users.TryGetValue(virtualId, out RoomUser user))
                return null;
            return user;
        }

        public RoomUser GetRoomUserByHabbo(int id)
        {
            if (_usersByHabboId != null && _usersByHabboId.TryGetValue(id, out RoomUser user))
                return user;

            return null;
        }

        public List<RoomUser> GetRoomUsers()
        {
            return GetUserList().Where(x => !x.IsBot).ToList();
        }

        public List<RoomUser> GetRoomUserByRank(int minRank)
        {
            var returnList = new List<RoomUser>();
            foreach (RoomUser user in GetUserList().ToList()) {
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
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            foreach (Pet pet in GetPets().ToList()) {
                if (pet == null)
                    continue;

                if (pet.DbState == PetDatabaseUpdateState.NeedsInsert) {
                    // Explicit PK (pet.PetId) preserved via raw insert; remaining bot columns rely on DB defaults.
                    db.Database.ExecuteSqlInterpolated($"INSERT INTO `bots` (`id`,`user_id`,`room_id`,`name`,`x`,`y`,`z`) VALUES ({pet.PetId},{pet.OwnerId},{pet.RoomId},{pet.Name},'0','0','0')");

                    // NOTE: the legacy code registered the @race/@color parameters under mismatched names
                    // (pet.PetId + "race"/"color"), so they never bound to the query and were inserted empty.
                    // Binding them correctly here to the pet's race/color (the evident intent).
                    db.Database.ExecuteSqlInterpolated($"INSERT INTO `bots_petdata` (`type`,`race`,`color`,`experience`,`energy`,`createstamp`,`nutrition`,`respect`) VALUES ({pet.Type},{pet.Race},{pet.Color},'0','100',{pet.CreationStamp},'0','0')");
                } else if (pet.DbState == PetDatabaseUpdateState.NeedsUpdate) {
                    //Surely this can be *99 better? // TODO
                    RoomUser user = GetRoomUserByVirtualId(pet.VirtualId);

                    uint petId = (uint)pet.PetId;
                    uint petRoomId = (uint)pet.RoomId;
                    int px = user != null ? user.X : 0;
                    int py = user != null ? user.Y : 0;
                    int pz = user != null ? (int)user.Z : 0;
                    db.Bots.Where(b => b.Id == petId)
                        .ExecuteUpdate(s => s
                            .SetProperty(b => b.RoomId, petRoomId)
                            .SetProperty(b => b.X, px)
                            .SetProperty(b => b.Y, py)
                            .SetProperty(b => b.Z, pz));

                    int petExperience = pet.Experience;
                    int petEnergy = pet.Energy;
                    int petNutrition = pet.Nutrition;
                    int petRespect = pet.Respect;
                    db.BotsPetdata.Where(d => d.Id == petId)
                        .ExecuteUpdate(s => s
                            .SetProperty(d => d.Experience, petExperience)
                            .SetProperty(d => d.Energy, petEnergy)
                            .SetProperty(d => d.Nutrition, petNutrition)
                            .SetProperty(d => d.Respect, petRespect));
                }

                pet.DbState = PetDatabaseUpdateState.Updated;
            }
        }

        private void UpdateBots()
        {
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            // NOTE: preserved as-is — GetRoomUsers() already filters out bots (Where(!IsBot)), so the
            // inner IsBot check never passes and this loop updates nothing. Faithful to the legacy behavior.
            foreach (RoomUser user in GetRoomUsers().ToList()) {
                if (user == null || !user.IsBot)
                    continue;

                if (user.IsBot) {
                    uint botId = (uint)user.BotData.BotId;
                    int bx = user.X;
                    int by = user.Y;
                    int bz = (int)user.Z;
                    string botName = user.BotData.Name;
                    string botLook = user.BotData.Look;
                    int botRot = user.BotData.Rot;
                    db.Bots.Where(b => b.Id == botId)
                        .ExecuteUpdate(s => s
                            .SetProperty(b => b.X, bx)
                            .SetProperty(b => b.Y, by)
                            .SetProperty(b => b.Z, bz)
                            .SetProperty(b => b.Name, botName)
                            .SetProperty(b => b.Look, botLook)
                            .SetProperty(b => b.Rotation, botRot));
                }
            }
        }

        public List<Pet> GetPets()
        {
            List<Pet> pets = new();
            foreach (RoomUser user in _pets.Values.ToList()) {
                if (user == null || !user.IsPet)
                    continue;

                pets.Add(user.PetData);
            }

            return pets;
        }

        public void SerializeStatusUpdates()
        {
            // Called every 50ms movement tick, so it stays allocation-free until there is
            // actually something to send.
            ICollection<RoomUser> roomUsers = GetUserList();

            if (roomUsers == null)
                return;

            List<RoomUser> users = null;

            foreach (RoomUser user in roomUsers) {
                if (user == null || !user.UpdateNeeded)
                    continue;

                user.UpdateNeeded = false;
                (users ??= []).Add(user);
            }

            if (users != null)
                _room.SendPacket(new UserUpdateComposer(users));
        }

        public void UpdateUserStatusses()
        {
            foreach (RoomUser user in GetUserList().ToList()) {
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

        // Tells the client the speed user is walking as.
        public void BroadcastWalkSpeed(RoomUser user)
        {
            if (user == null)
                return;

            user.LastSentWalkSpeed = user.WalkSpeed;
            _room.SendPacket(new AvatarWalkSpeedComposer(user.VirtualId, user.WalkSpeed, user.StepAnimationMs));
        }

        #region Pathing

        private void StartWalk(RoomUser user, long nowMs)
        {
            user.PathRecalcNeeded = false;

            Gamemap map = _room.GetGameMap();
            PathContext ctx = null;

            // A click that lands in the first moments of a tile can still turn the avatar around on
            // the spot. Plan that route first — it is the one the player actually asked for — and
            // only commit to abandoning the step in flight if it turns out to lead somewhere.
            if (user.SetStep && CanAbortStep(user, nowMs)) {
                ctx = map.CreatePathContext(user, GetOccupancySnapshot(nowMs));

                List<Vector3D> fromHere = PathFinder.FindPath(ctx, user.X, user.Y, user.Z, user.GoalX, user.GoalY);

                if (fromHere.Count > 1) {
                    Vector3D firstStep = fromHere[fromHere.Count - 2];

                    if (firstStep.X == user.SetX && firstStep.Y == user.SetY) {
                        // The step already in flight is the first step of the new route anyway.
                        // Drop that node and the remainder is the route from where it lands, so the
                        // walk carries straight on without a second search or a redundant turn.
                        fromHere.RemoveAt(fromHere.Count - 1);

                        user.Path = fromHere;
                        user.PathStep = 1;
                        user.IsWalking = true;
                        return;
                    }

                    AbortStep(user, nowMs);

                    user.Path = fromHere;
                    user.PathStep = 1;
                    user.IsWalking = true;
                    return;
                }

                // Nowhere to go from here — most often the tile we are standing on IS the new
                // target. Let the step land and walk back to it rather than snapping the avatar
                // backwards mid-slide.
            }

            // Where the avatar will be when it is next allowed to leave a tile. Mid-step that is
            // the tile it is walking INTO, so the new route is spliced on at the right end and is
            // ready to hand out the instant the step commits — no search happens at commit time,
            // and the route can never double back over the tile just walked.
            int fromX = user.SetStep ? user.SetX : user.X;
            int fromY = user.SetStep ? user.SetY : user.Y;
            double fromZ = user.SetStep ? user.SetZ : user.Z;

            if (user.GoalX == fromX && user.GoalY == fromY) {
                user.Path.Clear();
                return;
            }

            ctx ??= map.CreatePathContext(user, GetOccupancySnapshot(nowMs));

            List<Vector3D> path = PathFinder.FindPath(ctx, fromX, fromY, fromZ, user.GoalX, user.GoalY);

            if (path.Count > 1) {
                user.Path = path;
                user.PathStep = 1;
                user.IsWalking = true;
            } else
                user.Path.Clear();
        }

        /// <summary>
        /// Whether the step currently in flight may be thrown away so a fresh click can be answered
        /// from the tile the avatar is still standing on.
        /// </summary>
        private static bool CanAbortStep(RoomUser user, long nowMs)
        {
            // Rider and mount step as one, and bots gain nothing from click latency.
            if (user.IsBot || user.RidingHorse || user.HorseId > 0)
                return false;

            // At most one cancel per tile, so no amount of clicking can hold an avatar in place.
            if (user.StepAborted)
                return false;

            // The step is already due to land this tick; there is nothing left to cancel.
            if (nowMs >= user.NextStepMs)
                return false;

            int stepDuration = user.StepDurationMs;
            int window = Math.Min(MaxStepAbortWindowMs, (stepDuration * 2) / 5);

            if (user.StepCommittedMs <= 0 || (nowMs - user.StepCommittedMs) > window)
                return false;

            // Nothing to gain by turning towards the tile we are already walking onto.
            return user.GoalX != user.SetX || user.GoalY != user.SetY;
        }

        private void AbortStep(RoomUser user, long nowMs)
        {
            Gamemap map = _room.GetGameMap();

            // Issuing the step handed the target tile's square state to the user and marked that
            // tile occupied. Give both back, or the replacement step would stamp the abandoned
            // target's state onto the tile we are standing on.
            if (map.ValidTile(user.SetX, user.SetY) && map.ValidTile(user.X, user.Y)) {
                map.GameMap[user.SetX, user.SetY] = user.SqState;
                user.SqState = map.GameMap[user.X, user.Y];
            }

            user.SetStep = false;
            user.SetX = user.X;
            user.SetY = user.Y;
            user.SetZ = user.Z;
            user.StepAborted = true;

            // Let the replacement step leave immediately and take a full tile of its own; leaving
            // the old deadline in place would have charged it the remainder of the old step on top
            // of its own duration.
            user.NextStepMs = nowMs;
        }

        private byte[] GetOccupancySnapshot(long nowMs)
        {
            if (_room.RoomBlockingEnabled != 0)
                return null;

            Gamemap map = _room.GetGameMap();
            int size = map.Model.MapSizeX * map.Model.MapSizeY;

            byte[] occupancy = _occupancy;

            if (occupancy != null && occupancy.Length == size) {
                if (_occupancyTick == nowMs)
                    return occupancy;

                Array.Clear(occupancy, 0, size);
            } else
                occupancy = new byte[size];

            foreach (RoomUser other in GetUserList()) {
                if (other == null || !map.ValidTile(other.X, other.Y))
                    continue;

                int tile = (other.Y * map.Model.MapSizeX) + other.X;
                byte value = other.IsWalking ? PathContext.OccupancyWalking : PathContext.OccupancyStanding;

                if (occupancy[tile] < value)
                    occupancy[tile] = value;
            }

            _occupancy = occupancy;
            _occupancyTick = nowMs;

            return occupancy;
        }

        private static void DrawnTile(RoomUser user, long nowMs, out int x, out int y, out double z)
        {
            if (!user.SetStep || user.StepCommittedMs <= 0) {
                x = user.X;
                y = user.Y;
                z = user.Z;
                return;
            }

            bool pastHalfway = (nowMs - user.StepCommittedMs) * 2 >= user.StepDurationMs;

            x = pastHalfway ? user.SetX : user.X;
            y = pastHalfway ? user.SetY : user.Y;
            z = pastHalfway ? user.SetZ : user.Z;
        }

        private void UpdatePressurePlates(long nowMs)
        {
            Item[] plates = _room.GetRoomItemHandler().GetPressurePlates();

            if (plates.Length == 0)
                return;

            foreach (Item plate in plates) {
                if (plate == null)
                    continue;

                bool pressed = false;

                foreach (RoomUser user in GetUserList()) {
                    if (user == null)
                        continue;

                    DrawnTile(user, nowMs, out int x, out int y, out double z);

                    // A plate under a walkway somebody is passing above is not being stood on.
                    if (!plate.IsAtUserLevel(z))
                        continue;

                    if (!Covers(plate, x, y))
                        continue;

                    pressed = true;
                    break;
                }

                plate.SetPressurePlate(pressed);
            }
        }

        private static bool Covers(Item item, int x, int y)
        {
            if (item.GetX == x && item.GetY == y)
                return true;

            foreach (ThreeDCoord tile in item.GetAffectedTiles.Values) {
                if (tile.X == x && tile.Y == y)
                    return true;
            }

            return false;
        }

        #endregion

        public void OnCycle(bool fullCycle)
        {
            int userCounter = 0;
            long nowMs = PlusEnvironment.MonotonicMs();

            try {
                // Allocated only when someone actually needs kicking, which is almost never.
                List<RoomUser> ToRemove = null;

                foreach (RoomUser user in GetUserList()) {
                    if (user == null)
                        continue;

                    bool updated = false;
                    bool invalidStep = false;
                    bool statusChanged = false;

                    // Housekeeping (idle/carry/spam timers, autokick, freeze, rollers) stays on 500ms
                    if (fullCycle) {
                        if (!IsValid(user)) {
                            if (user.GetClient() != null)
                                RemoveUserFromRoom(user.GetClient(), false);
                            else
                                RemoveRoomUser(user);
                        }

                        if (user.NeedsAutoKick && (ToRemove == null || !ToRemove.Contains(user))) {
                            (ToRemove ??= []).Add(user);
                            continue;
                        }

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

                        if (user.EffectReapplyTimer > 0) {
                            user.EffectReapplyTimer--;
                            if (user.EffectReapplyTimer <= 0 && !user.IsDancing) {
                                int effect = user.GetClient()?.GetHabbo()?.Effects()?.CurrentEffect ?? 0;
                                if (effect > 0)
                                    _room.SendPacket(new Plus.Communication.Packets.Outgoing.Rooms.Avatar.AvatarEffectComposer(user.VirtualId, effect));
                            }
                        }

                        if (user.LastSentWalkSpeed != user.WalkSpeed)
                            BroadcastWalkSpeed(user);

                        if (_room.GotFreeze())
                            _room.GetFreeze().CycleUser(user);

                        if (user.IsRolling) {
                            if (user.RollerDelay <= 0) {
                                UpdateUserStatus(user, false);
                                user.IsRolling = false;
                            } else
                                user.RollerDelay--;
                        }
                    }

                    // Plan first, move second. A click is answered on the tick it arrives even when
                    // the avatar is mid-slide and cannot legally leave its tile yet: the route is
                    // searched from the tile the step in flight is walking into, so the moment that
                    // step lands the next one is already sitting there waiting to be issued. The
                    // search may also cancel the step outright (see CanAbortStep), which is why the
                    // step gate below is read afterwards and not before.
                    if (user.PathRecalcNeeded)
                        StartWalk(user, nowMs);

                    // Per-user step gate: a fresh click starts walking within one 50ms tick, but
                    // consecutive steps keep the 500ms-per-tile pace the client animates at.
                    bool canStep = nowMs >= user.NextStepMs;

                    if (canStep && user.SetStep) {
                        if (_room.GetGameMap().IsValidStep2(user, new Vector2D(user.X, user.Y), new Vector2D(user.SetX, user.SetY), user.SetZ, (user.GoalX == user.SetX && user.GoalY == user.SetY), user.AllowOverride)) {
                            if (!user.RidingHorse)
                                _room.GetGameMap().UpdateUserMovement(new Point(user.Coordinate.X, user.Coordinate.Y), new Point(user.SetX, user.SetY), user);

                            // GetCoordinatedItems already hands back a fresh list; copying it again
                            // just to iterate was pure allocation, twice per step per user.
                            List<Item> items = _room.GetGameMap().GetCoordinatedItems(new Point(user.X, user.Y));
                            foreach (Item item in items) {
                                // Furniture the user merely passed underneath was never stepped on,
                                // so it can't be stepped off either.
                                if (item.IsAtUserLevel(user.Z, SameLevelTolerance))
                                    item.UserWalksOffFurni(user);
                            }

                            user.PrevX = user.X;
                            user.PrevY = user.Y;
                            user.PrevZ = user.Z;
                            user.StepCommittedMs = nowMs;

                            if (!user.IsBot) {
                                user.X = user.SetX;
                                user.Y = user.SetY;
                                user.Z = user.SetZ;
                            } else if (user.IsBot && !user.RidingHorse) {
                                user.X = user.SetX;
                                user.Y = user.SetY;
                                user.Z = user.SetZ;
                            }

                            // Landed on a new tile, so the next one may be turned around again.
                            user.StepAborted = false;

                            if (!user.IsBot && user.RidingHorse) {
                                RoomUser horse = GetRoomUserByVirtualId(user.HorseId);
                                if (horse != null) {
                                    horse.X = user.SetX;
                                    horse.Y = user.SetY;
                                }
                            }

                            bool endOfWalk = user.GoalX == user.X && user.GoalY == user.Y;

                            List<Item> Items = _room.GetGameMap().GetCoordinatedItems(new Point(user.X, user.Y));
                            foreach (Item item in Items) {
                                // Walking under a raised platform is not walking on it.
                                if (!item.IsAtUserLevel(user.Z, SameLevelTolerance))
                                    continue;

                                item.UserWalksOnFurni(user);

                                if (!user.IsBot && endOfWalk && item.GetBaseItem()?.InteractionType == InteractionType.Arrow)
                                    Plus.HabboHotel.Items.Interactor.InteractorArrowTeleport.BeginDelayedTeleport(user, item);
                            }

                            UpdateUserStatus(user, true);
                        } else
                            invalidStep = true;

                        user.SetStep = false;
                    }

                    // The step that just landed can invalidate the rest of the route — a door swung
                    // shut, someone parked on the next tile. Re-planning right here costs one
                    // search and keeps the walk going on the same tick instead of stalling a tile.
                    if (user.PathRecalcNeeded) {
                        StartWalk(user, nowMs);

                        if (user.IsWalking && user.Path.Count > 1)
                            invalidStep = false;
                    }

                    if (canStep && user.IsWalking && !user.Freezed) {
                        if (invalidStep || (user.PathStep >= user.Path.Count) || (user.GoalX == user.X && user.GoalY == user.Y)) //No path found, or reached goal (:
                        {
                            user.IsWalking = false;
                            user.NextStepMs = 0;
                            user.RemoveStatus("mv");
                            statusChanged = true;

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
                                    mascotaVinculada.NextStepMs = 0;
                                    mascotaVinculada.RemoveStatus("mv");
                                    mascotaVinculada.UpdateNeeded = true;
                                }
                            }
                        } else {
                            // Tiers change how long a tile takes, never which tiles get walked, so
                            // every square of the route is stepped on regardless of walk speed.
                            Vector3D nextStep = user.Path[(user.Path.Count - user.PathStep) - 1];
                            user.PathStep++;

                            int nextX = nextStep.X;
                            int nextY = nextStep.Y;

                            if (_room.GetGameMap().IsValidStep2(user, new Vector2D(user.X, user.Y), new Vector2D(nextX, nextY), nextStep.Z, (user.GoalX == nextX && user.GoalY == nextY), user.AllowOverride)) {
                                // Height comes from the path, not from the top of the tile: with 3D
                                // pathing the route may deliberately pass underneath furniture.
                                double nextZ = nextStep.Z;

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

                                // Schedule from the step that just came due, not from now. Ticks
                                // arrive a few milliseconds late, and rebasing on "now" folded that
                                // lateness into every following step until the avatar was visibly
                                // waiting between tiles. Falling a whole step behind resyncs.
                                long nextStepTarget = user.NextStepMs + user.StepDurationMs;
                                user.NextStepMs = nextStepTarget > nowMs ? nextStepTarget : nowMs + user.StepDurationMs;

                                if (user.StepCommittedMs != nowMs) {
                                    user.PrevX = user.X;
                                    user.PrevY = user.Y;
                                    user.PrevZ = user.Z;
                                }
                                user.StepCommittedMs = nowMs;

                                UpdateUserEffect(user, user.SetX, user.SetY);

                                updated = true;
                                statusChanged = true;

                                if (user.RidingHorse && user.IsPet == false && !user.IsBot) {
                                    RoomUser horse = GetRoomUserByVirtualId(user.HorseId);
                                    if (horse != null) {
                                        horse.RotBody = newRot;
                                        horse.RotHead = newRot;

                                        horse.SetStep = true;
                                        horse.SetX = nextX;
                                        horse.SetY = nextY;
                                        horse.SetZ = nextZ;
                                        // Pace the horse like its rider so the fast movement tick
                                        // doesn't commit the step (and strip "mv") after 50ms.
                                        horse.NextStepMs = user.NextStepMs;
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

                        if (!user.RidingHorse && statusChanged)
                            user.UpdateNeeded = true;
                    } else if (canStep && !user.SetStep) {
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

                    if (fullCycle) {
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
                }

                // Escorted suspects move fused with their officer. This runs AFTER the user loop so
                // the suspect's freshly-written "mv" status can't be stripped by their own (idle)
                // processing pass before it's serialized to clients — that strip is what turned the
                // escort into an unanimated teleport.
                PlusEnvironment.GetPoliceManager()?.SyncEscorts(_room);

                // After everything that can move somebody this tick, so a plate never reports a
                // position that is already one tick stale.
                UpdatePressurePlates(nowMs);

                if (fullCycle) {
                    foreach (RoomUser toRemove in ToRemove ?? Enumerable.Empty<RoomUser>()) {
                        GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(toRemove.HabboId);
                        if (client != null) {
                            RemoveUserFromRoom(client, true);
                        } else
                            RemoveRoomUser(toRemove);
                    }

                    if (UserCount != userCounter)
                        UpdateUserCount(userCounter);
                }
            } catch (Exception e) {
                ExceptionLogger.LogCriticalException(e);
            }
        }

        private static double ResolveSurfaceUnderUser(List<Item> itemsOnSquare, double surface)
        {
            foreach (Item item in itemsOnSquare) {
                ItemData data = item?.GetBaseItem();

                if (data == null || data.Walkable)
                    continue;

                bool baseAtOrBelowFeet = item.GetZ <= surface + RoomHeightMap.Epsilon;
                bool topAboveFeet = item.TotalHeight > surface + RoomHeightMap.Epsilon;

                if (!baseAtOrBelowFeet || !topAboveFeet)
                    continue;

                double standOn = item.ContactHeight;

                if (standOn > surface && standOn - surface <= RoomHeightMap.MaxClimbHeight)
                    surface = standOn;
            }

            return surface;
        }

        public void UpdateUserStatus(RoomUser user, bool cycleGameItems)
        {
            if (user == null)
                return;

            try {
                bool isBot = user.IsBot;
                if (isBot)
                    cycleGameItems = false;

                if (PlusEnvironment.GetUnixTimestamp() > PlusEnvironment.GetUnixTimestamp() + user.SignTime) {
                    if (user.Statusses.ContainsKey("sign")) {
                        user.Statusses.Remove("sign");
                        user.UpdateNeeded = true;
                    }
                }

                user.Statusses.TryGetValue("sit", out string poseSitBefore);
                user.Statusses.TryGetValue("lay", out string poseLayBefore);
                double poseZBefore = user.Z;
                int poseRotBodyBefore = user.RotBody;
                int poseRotHeadBefore = user.RotHead;

                if ((user.Statusses.ContainsKey("lay") && !user.IsLying) || (user.Statusses.ContainsKey("sit") && !user.IsSitting)) {
                    if (user.Statusses.ContainsKey("lay"))
                        user.Statusses.Remove("lay");
                    if (user.Statusses.ContainsKey("sit"))
                        user.Statusses.Remove("sit");
                } else if (user.IsLying || user.IsSitting)
                    return;

                Gamemap gameMap = _room.GetGameMap();
                List<Item> itemsOnSquare = gameMap.GetAllRoomItemForSquare(user.X, user.Y) ?? [];
                RoomHeightMap heightMap = gameMap.GetHeightMap();
                double newZ;

                if (heightMap.LevelCountAt(user.X, user.Y) > 0)
                    newZ = heightMap.SurfaceNear(user.X, user.Y, user.Z);
                else if (itemsOnSquare.Count != 0)
                    newZ = gameMap.SqAbsoluteHeight(user.X, user.Y, itemsOnSquare); // tile the height map knows nothing about
                else
                    newZ = gameMap.Model.SqFloorHeight[user.X, user.Y];

                if (itemsOnSquare.Count != 0)
                    newZ = ResolveSurfaceUnderUser(itemsOnSquare, newZ);

                if (user.RidingHorse && user.IsPet == false)
                    newZ += 1;

                user.Z = newZ;

                DynamicRoomModel model = gameMap.Model;
                if (model.SqState[user.X, user.Y] == SquareState.Seat) {
                    if (!user.Statusses.ContainsKey("sit"))
                        user.Statusses.Add("sit", "1.0");
                    user.Z = model.SqFloorHeight[user.X, user.Y];
                    user.RotHead = model.SqSeatRot[user.X, user.Y];
                    user.RotBody = model.SqSeatRot[user.X, user.Y];
                }

                if (itemsOnSquare.Count == 0)
                    user.LastItem = null;

                double standingLevel = user.Z;
                Item seat = null;

                foreach (Item item in itemsOnSquare) {
                    if (item == null || !item.GetBaseItem().IsSeat)
                        continue;

                    if (!item.IsAtUserLevel(standingLevel, SameLevelTolerance))
                        continue;

                    if (seat == null || item.GetZ > seat.GetZ || (item.GetZ == seat.GetZ && item.Id < seat.Id))
                        seat = item;
                }

                if (seat != null) {
                    string sitHeight = TextHandling.GetString(seat.EffectiveHeight);

                    if (!user.Statusses.TryGetValue("sit", out string currentSit) || currentSit != sitHeight)
                        user.Statusses["sit"] = sitHeight;

                    user.Z = seat.GetZ;
                    user.RotHead = seat.Rotation;
                    user.RotBody = seat.Rotation;
                }

                foreach (Item item in itemsOnSquare) {
                    if (item == null)
                        continue;

                    // Only furniture the user is actually in contact with counts. Anything floating
                    // clear of them is something they walked underneath, not something they are
                    // standing on, sitting in, or triggering.
                    if (!item.IsAtUserLevel(standingLevel, SameLevelTolerance))
                        continue;

                    switch (item.GetBaseItem().InteractionType) {
                        #region Beds & Tents

                        case InteractionType.Bed:
                        case InteractionType.TentSmall: {
                                string layHeight = TextHandling.GetString(item.EffectiveHeight) + " null";

                                if (!user.Statusses.TryGetValue("lay", out string currentLay) || currentLay != layHeight)
                                    user.Statusses["lay"] = layHeight;

                                user.Z = item.GetZ;
                                user.RotHead = item.Rotation;
                                user.RotBody = item.Rotation;
                                break;
                            }

                        #endregion

                        #region Banzai Gates

                        case InteractionType.BanzaiGateGreen:
                        case InteractionType.BanzaiGateBlue:
                        case InteractionType.BanzaiGateRed:
                        case InteractionType.BanzaiGateYellow: {
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
                        case InteractionType.FreezeBlueGate: {
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

                        case InteractionType.BanzaiTele: {
                                if (user.Statusses.ContainsKey("mv"))
                                    _room.GetGameItemHandler().OnTeleportRoomUserEnter(user, item);
                                break;
                            }

                        #endregion

                        #region Football Gate

                        #endregion

                        #region Effects

                        // break, not return: an effect tile that has nothing to do is not a reason
                        // to abandon the rest of the square and skip the pose flush below.
                        case InteractionType.Effect: {
                                if (!user.IsBot) {
                                    if (item.GetBaseItem() == null || user.GetClient() == null || user.GetClient().GetHabbo() == null || user.GetClient().GetHabbo().Effects() == null)
                                        break;

                                    if (item.GetBaseItem().EffectId == 0 && user.GetClient().GetHabbo().Effects().CurrentEffect == 0)
                                        break;

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

                if (user.IsSitting && user.TeleportEnabled)
                    user.Z -= 0.35;

                // One flag for the whole rebuild. Anything the passes above actually moved gets
                // broadcast; a pose that came out identical costs nothing.
                user.Statusses.TryGetValue("sit", out string poseSitAfter);
                user.Statusses.TryGetValue("lay", out string poseLayAfter);

                if (poseSitAfter != poseSitBefore || poseLayAfter != poseLayBefore ||
                    user.Z != poseZBefore || user.RotBody != poseRotBodyBefore || user.RotHead != poseRotHeadBefore)
                    user.UpdateNeeded = true;

                if (cycleGameItems) {
                    if (_room.GotSoccer())
                        _room.GetSoccer().OnUserWalk(user);

                    if (_room.GotBanzai())
                        _room.GetBanzai().OnUserWalk(user);

                    if (_room.GotFreeze())
                        _room.GetFreeze().OnUserWalk(user);
                }
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }
        }

        private void UpdateUserEffect(RoomUser user, int x, int y)
        {
            if (user == null || user.IsBot || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            try {
                byte newCurrentUserItemEffect = _room.GetGameMap().EffectMap[x, y];
                if (newCurrentUserItemEffect > 0) {
                    if (user.GetClient().GetHabbo().Effects().CurrentEffect == 0)
                        user.CurrentItemEffect = ItemEffectType.None;

                    ItemEffectType type = ByteToItemEffectEnum.Parse(newCurrentUserItemEffect);
                    if (type != user.CurrentItemEffect) {
                        switch (type) {
                            case ItemEffectType.IceSkates: {
                                    user.GetClient().GetHabbo().Effects().ApplyEffect(user.GetClient().GetHabbo().Gender == "M" ? 38 : 39);
                                    user.CurrentItemEffect = ItemEffectType.IceSkates;
                                    break;
                                }

                            case ItemEffectType.NormalSkates: {
                                    user.GetClient().GetHabbo().Effects().ApplyEffect(user.GetClient().GetHabbo().Gender == "M" ? 55 : 56);
                                    user.CurrentItemEffect = type;
                                    break;
                                }
                            case ItemEffectType.Swim: {
                                    user.GetClient().GetHabbo().Effects().ApplyEffect(29);
                                    user.CurrentItemEffect = type;
                                    break;
                                }
                            case ItemEffectType.SwimLow: {
                                    user.GetClient().GetHabbo().Effects().ApplyEffect(30);
                                    user.CurrentItemEffect = type;
                                    break;
                                }
                            case ItemEffectType.SwimHalloween: {
                                    user.GetClient().GetHabbo().Effects().ApplyEffect(37);
                                    user.CurrentItemEffect = type;
                                    break;
                                }

                            case ItemEffectType.None: {
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
            } catch {
            }
        }

        public int PetCount { get; private set; }

        /// <summary>
        /// Everyone in the room, as a snapshot that is safe to iterate without copying — including
        /// while the loop itself removes users. Callers do not need <c>.ToList()</c>.
        /// </summary>
        public ICollection<RoomUser> GetUserList()
        {
            return _userSnapshot;
        }

        private void RebuildUserSnapshot()
        {
            ConcurrentDictionary<int, RoomUser> users = _users;
            _userSnapshot = users == null ? [] : users.Values.ToArray();
        }

        public void Dispose()
        {
            UpdatePets();
            UpdateBots();

            _room.UsersNow = 0;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                int roomId = _room.Id;
                db.Rooms.Where(r => r.Id == roomId)
                    .ExecuteUpdate(s => s.SetProperty(r => r.UsersNow, 0));
            }

            _users.Clear();
            _userSnapshot = [];
            _pets.Clear();
            _bots.Clear();
            _usersByHabboId.Clear();

            UserCount = 0;
            PetCount = 0;

            _users = null;
            _pets = null;
            _bots = null;
            _usersByHabboId = null;
            _room = null;
        }
    }
}