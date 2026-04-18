using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Core;
using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Data.Moodlight;
using Plus.HabboHotel.Items.Data.Toner;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Rooms.PathFinding;

namespace Plus.HabboHotel.Rooms
{
    public class RoomItemHandling
    {
        private readonly Room _room;

        public int HopperCount;
        private int _mRollerSpeed;
        private int _mRollerCycle;

        private readonly ConcurrentDictionary<int, Item> _movedItems;

        private readonly ConcurrentDictionary<int, Item> _rollers;
        private readonly ConcurrentDictionary<int, Item> _wallItems;
        private readonly ConcurrentDictionary<int, Item> _floorItems;

        private readonly List<int> _rollerItemsMoved;
        private readonly List<int> _rollerUsersMoved;
        private readonly List<MessageComposer> _rollerMessages;

        private ConcurrentQueue<Item> _roomItemUpdateQueue;

        public RoomItemHandling(Room room)
        {
            _room = room;

            HopperCount = 0;
            GotRollers = false;
            _mRollerSpeed = 4;
            _mRollerCycle = 0;

            _movedItems = new ConcurrentDictionary<int, Item>();

            _rollers = new ConcurrentDictionary<int, Item>();
            _wallItems = new ConcurrentDictionary<int, Item>();
            _floorItems = new ConcurrentDictionary<int, Item>();

            _rollerItemsMoved = new List<int>();
            _rollerUsersMoved = new List<int>();
            _rollerMessages = new List<MessageComposer>();

            _roomItemUpdateQueue = new ConcurrentQueue<Item>();
        }

        public void TryAddRoller(int itemId, Item roller)
        {
            _rollers.TryAdd(itemId, roller);
        }

        public bool GotRollers { get; set; }

        public void QueueRoomItemUpdate(Item item)
        {
            _roomItemUpdateQueue.Enqueue(item);
        }

        public void SetSpeed(int p)
        {
            _mRollerSpeed = p;
        }

        public string WallPositionCheck(string wallPosition)
        {
            //:w=3,2 l=9,63 l
            try
            {
                if (wallPosition.Contains(Convert.ToChar(13)))
                {
                    return null;
                }

                if (wallPosition.Contains(Convert.ToChar(9)))
                {
                    return null;
                }

                string[] posD = wallPosition.Split(' ');
                if (posD[2] != "l" && posD[2] != "r")
                    return null;

                string[] widD = posD[0].Substring(3).Split(',');
                int widthX = int.Parse(widD[0]);
                int widthY = int.Parse(widD[1]);
                if (widthX < -1000 || widthY < -1 || widthX > 700 || widthY > 700)
                    return null;

                string[] lenD = posD[1].Substring(2).Split(',');
                int lengthX = int.Parse(lenD[0]);
                int lengthY = int.Parse(lenD[1]);
                if (lengthX < -1 || lengthY < -1000 || lengthX > 700 || lengthY > 700)
                    return null;

                return ":w=" + widthX + "," + widthY + " " + "l=" + lengthX + "," + lengthY + " " + posD[2];
            }
            catch
            {
                return null;
            }
        }

        public void LoadFurniture()
        {
            if (_floorItems.Count > 0)
                _floorItems.Clear();
            if (_wallItems.Count > 0)
                _wallItems.Clear();

            List<Item> items = ItemLoader.GetItemsForRoom(_room.Id, _room);
            foreach (Item item in items.ToList())
            {
                if (item == null)
                    continue;

                if (item.UserId == 0)
                {
                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `items` SET `user_id` = @UserId WHERE `id` = @ItemId LIMIT 1");
                        dbClient.AddParameter("ItemId", item.Id);
                        dbClient.AddParameter("UserId", _room.OwnerId);
                        dbClient.RunQuery();
                    }
                }

                if (item.IsFloorItem)
                {
                    if (!_room.GetGameMap().ValidTile(item.GetX, item.GetY))
                    {
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = '" + item.Id + "' LIMIT 1");
                        }

                        GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(item.UserId);
                        if (client != null)
                        {
                            client.GetHabbo().GetInventoryComponent().AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, item.LimitedNo, item.LimitedTot);
                            client.GetHabbo().GetInventoryComponent().UpdateItems(false);
                        }

                        continue;
                    }

                    if (!_floorItems.ContainsKey(item.Id))
                        _floorItems.TryAdd(item.Id, item);
                }
                else if (item.IsWallItem)
                {
                    if (string.IsNullOrWhiteSpace(item.WallCoord))
                    {
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `items` SET `wall_pos` = @WallPosition WHERE `id` = '" + item.Id + "' LIMIT 1");
                            dbClient.AddParameter("WallPosition", ":w=0,2 l=11,53 l");
                            dbClient.RunQuery();
                        }

                        item.WallCoord = ":w=0,2 l=11,53 l";
                    }

                    try
                    {
                        item.WallCoord = WallPositionCheck(":" + item.WallCoord.Split(':')[1]);
                    }
                    catch
                    {
                        using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `items` SET `wall_pos` = @WallPosition WHERE `id` = '" + item.Id + "' LIMIT 1");
                            dbClient.AddParameter("WallPosition", ":w=0,2 l=11,53 l");
                            dbClient.RunQuery();
                        }

                        item.WallCoord = ":w=0,2 l=11,53 l";
                    }

                    if (!_wallItems.ContainsKey(item.Id))
                        _wallItems.TryAdd(item.Id, item);
                }
            }

            foreach (Item item in _floorItems.Values.ToList())
            {
                if (item.IsRoller)
                {
                    GotRollers = true;
                }
                else if (item.GetBaseItem().InteractionType == InteractionType.Moodlight)
                {
                    if (_room.MoodlightData == null)
                        _room.MoodlightData = new MoodlightData(item.Id);
                }
                else if (item.GetBaseItem().InteractionType == InteractionType.Toner)
                {
                    if (_room.TonerData == null)
                        _room.TonerData = new TonerData(item.Id);
                }
                else if (item.IsWired)
                {
                    if (_room?.GetWired() == null)
                        continue;

                    _room.GetWired().LoadWiredBox(item);
                }
                else if (item.GetBaseItem().InteractionType == InteractionType.Hopper)
                    HopperCount++;
            }
        }

        public Item GetItem(int pId)
        {
            if (_floorItems != null && _floorItems.ContainsKey(pId))
            {
                if (_floorItems.TryGetValue(pId, out Item item))
                    return item;
            }
            else if (_wallItems != null && _wallItems.ContainsKey(pId))
            {
                if (_wallItems.TryGetValue(pId, out Item item))
                    return item;
            }

            return null;
        }

        public void RemoveFurniture(GameClient session, int id)
        {
            Item item = GetItem(id);
            if (item == null)
                return;

            if (item.GetBaseItem().InteractionType == InteractionType.FootballGate)
                _room.GetSoccer().UnRegisterGate(item);

            if (item.GetBaseItem().InteractionType != InteractionType.Gift)
                item.Interactor.OnRemove(session, item);

            if (item.GetBaseItem().InteractionType == InteractionType.GuildGate)
            {
                item.UpdateCounter = 0;
                item.UpdateNeeded = false;
            }

            RemoveRoomItem(item);
        }

        private void RemoveRoomItem(Item item)
        {
            if (item.IsFloorItem)
                _room.SendPacket(new ObjectRemoveComposer(item, item.UserId));
            else if (item.IsWallItem)
                _room.SendPacket(new ItemRemoveComposer(item, item.UserId));

            //TODO: Recode this specific part
            if (item.IsWallItem)
                _wallItems.TryRemove(item.Id, out item);
            else
            {
                _floorItems.TryRemove(item.Id, out item);
                //mFloorItems.OnCycle();
                _room.GetGameMap().RemoveFromMap(item);
            }

            RemoveItem(item);
            _room.GetGameMap().GenerateMaps();
            _room.GetRoomUserManager().UpdateUserStatusses();
        }

        private List<MessageComposer> CycleRollers()
        {
            if (!GotRollers)
                return new List<MessageComposer>();

            if (_mRollerCycle >= _mRollerSpeed || _mRollerSpeed == 0)
            {
                _rollerItemsMoved.Clear();
                _rollerUsersMoved.Clear();
                _rollerMessages.Clear();

                foreach (Item roller in _rollers.Values.ToList())
                {
                    if (roller == null)
                        continue;

                    Point nextSquare = roller.SquareInFront;

                    List<Item> itemsOnRoller = _room.GetGameMap().GetRoomItemForSquare(roller.GetX, roller.GetY, roller.GetZ);
                    List<Item> itemsOnNext = _room.GetGameMap().GetAllRoomItemForSquare(nextSquare.X, nextSquare.Y).ToList();

                    if (itemsOnRoller.Count > 10)
                        itemsOnRoller = _room.GetGameMap().GetRoomItemForSquare(roller.GetX, roller.GetY, roller.GetZ).Take(10).ToList();

                    bool nextSquareIsRoller = (itemsOnNext.Count(x => x.GetBaseItem().InteractionType == InteractionType.Roller) > 0);
                    bool nextRollerClear = true;

                    double nextZ = 0.0;
                    bool nextRoller = false;

                    foreach (Item item in itemsOnNext.ToList())
                    {
                        if (item.IsRoller)
                        {
                            if (item.TotalHeight > nextZ)
                                nextZ = item.TotalHeight;

                            nextRoller = true;
                        }
                    }

                    if (nextRoller)
                    {
                        foreach (Item item in itemsOnNext.ToList())
                        {
                            if (item.TotalHeight > nextZ)
                                nextRollerClear = false;
                        }
                    }

                    if (itemsOnRoller.Count > 0)
                    {
                        foreach (Item rItem in itemsOnRoller.ToList())
                        {
                            if (rItem == null)
                                continue;

                            if (!_rollerItemsMoved.Contains(rItem.Id) && _room.GetGameMap().CanRollItemHere(nextSquare.X, nextSquare.Y) && nextRollerClear && roller.GetZ < rItem.GetZ && _room.GetRoomUserManager().GetUserForSquare(nextSquare.X, nextSquare.Y) == null)
                            {
                                if (!nextSquareIsRoller)
                                    nextZ = rItem.GetZ - roller.GetBaseItem().Height;
                                else
                                    nextZ = rItem.GetZ;

                                _rollerMessages.Add(UpdateItemOnRoller(rItem, nextSquare, roller.Id, nextZ));
                                _rollerItemsMoved.Add(rItem.Id);
                            }
                        }
                    }

                    RoomUser rollerUser = _room.GetGameMap().GetRoomUsers(roller.Coordinate).FirstOrDefault();

                    if (rollerUser != null && !rollerUser.IsWalking && nextRollerClear && _room.GetGameMap().IsValidStep(new Vector2D(roller.GetX, roller.GetY), new Vector2D(nextSquare.X, nextSquare.Y), true, false, true) && _room.GetGameMap().CanRollItemHere(nextSquare.X, nextSquare.Y) && _room.GetGameMap().GetFloorStatus(nextSquare) != 0)
                    {
                        if (!_rollerUsersMoved.Contains(rollerUser.HabboId))
                        {
                            if (!nextSquareIsRoller)
                                nextZ = rollerUser.Z - roller.GetBaseItem().Height;
                            else
                                nextZ = rollerUser.Z;

                            rollerUser.IsRolling = true;
                            rollerUser.RollerDelay = 1;

                            _rollerMessages.Add(UpdateUserOnRoller(rollerUser, nextSquare, roller.Id, nextZ));
                            _rollerUsersMoved.Add(rollerUser.HabboId);
                        }
                    }
                }

                _mRollerCycle = 0;
                return _rollerMessages;
            }

            _mRollerCycle++;

            return new List<MessageComposer>();
        }

        public MessageComposer UpdateItemOnRoller(Item pItem, Point nextCoord, int pRolledId, double nextZ)
        {
            var mMessage = new SlideObjectBundleComposer(pItem.GetX, pItem.GetY, pItem.GetZ, nextCoord.X, nextCoord.Y, nextZ, pRolledId, 0, pItem.Id);
            SetFloorItem(pItem, nextCoord.X, nextCoord.Y, nextZ);
            return mMessage;
        }

        public MessageComposer UpdateUserOnRoller(RoomUser pUser, Point pNextCoord, int pRollerId, double nextZ)
        {
            SlideObjectBundleComposer mMessage = new(pUser.X, pUser.Y, pUser.Z, pNextCoord.X,
                pNextCoord.Y, nextZ, pRollerId, pUser.VirtualId, -1);

            _room.GetGameMap()
                .UpdateUserMovement(new Point(pUser.X, pUser.Y), new Point(pNextCoord.X, pNextCoord.Y), pUser);
            _room.GetGameMap().GameMap[pUser.X, pUser.Y] = 1;
            pUser.X = pNextCoord.X;
            pUser.Y = pNextCoord.Y;
            pUser.Z = nextZ;

            _room.GetGameMap().GameMap[pUser.X, pUser.Y] = 0;

            if (pUser != null && pUser.GetClient() != null && pUser.GetClient().GetHabbo() != null)
            {
                List<Item> items = _room.GetGameMap().GetRoomItemForSquare(pNextCoord.X, pNextCoord.Y);
                foreach (Item IItem in items.ToList())
                {
                    if (IItem == null)
                        continue;

                    _room.GetWired().TriggerEvent(WiredBoxType.TriggerWalkOnFurni, pUser.GetClient().GetHabbo(), IItem);
                }

                Item item = _room.GetRoomItemHandler().GetItem(pRollerId);
                if (item != null)
                {
                    _room.GetWired().TriggerEvent(WiredBoxType.TriggerWalkOffFurni, pUser.GetClient().GetHabbo(), item);
                }
            }

            return mMessage;
        }

        private void SaveFurniture()
        {
            try
            {
                if (_movedItems.Count > 0)
                {
                    // TODO: Big string builder?
                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        foreach (Item item in _movedItems.Values.ToList())
                        {
                            if (!string.IsNullOrEmpty(item.ExtraData))
                            {
                                dbClient.SetQuery("UPDATE `items` SET `extra_data` = @edata" + item.Id + " WHERE `id` = '" + item.Id + "' LIMIT 1");
                                dbClient.AddParameter("edata" + item.Id, item.ExtraData);
                                dbClient.RunQuery();
                            }

                            if (item.IsWallItem && (!item.GetBaseItem().ItemName.Contains("wallpaper_single") || !item.GetBaseItem().ItemName.Contains("floor_single") || !item.GetBaseItem().ItemName.Contains("landscape_single")))
                            {
                                dbClient.SetQuery("UPDATE `items` SET `wall_pos` = @wallPos WHERE `id` = '" + item.Id + "' LIMIT 1");
                                dbClient.AddParameter("wallPos", item.WallCoord);
                                dbClient.RunQuery();
                            }

                            dbClient.RunQuery("UPDATE `items` SET `x` = '" + item.GetX + "', `y` = '" + item.GetY + "', `z` = '" + item.GetZ + "', `rot` = '" + item.Rotation + "' WHERE `id` = '" + item.Id + "' LIMIT 1");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionLogger.LogCriticalException(e);
            }
        }

        public bool SetFloorItem(GameClient session, Item item, int newX, int newY, int newRot, bool newItem, bool onRoller, bool sendMessage, bool updateRoomUserStatuses = false, double height = -1)
        {
            newRot = NormalizeFloorRotation(newRot);

            double customBuildHeight = session?.GetHabbo()?.CustomBuildHeight ?? -1;
            if (customBuildHeight > -1)
                height = customBuildHeight;

            bool needsReAdd = false;

            if (newItem)
            {
                if (item.IsWired)
                {
                    if (item.GetBaseItem().WiredType == WiredBoxType.EffectRegenerateMaps && _room.GetRoomItemHandler().GetFloor.Count(x => x.GetBaseItem().WiredType == WiredBoxType.EffectRegenerateMaps) > 0)
                        return false;
                }
            }

            List<Item> itemsOnTile = GetFurniObjects(newX, newY);
            if (item.GetBaseItem().InteractionType == InteractionType.Roller && itemsOnTile.Count(x => x.GetBaseItem().InteractionType == InteractionType.Roller && x.Id != item.Id) > 0)
                return false;

            if (!newItem)
                needsReAdd = _room.GetGameMap().RemoveFromMap(item);

            Dictionary<int, ThreeDCoord> affectedTiles = Gamemap.GetAffectedTiles(item.GetBaseItem().Length, item.GetBaseItem().Width, newX, newY, newRot);

            if (!_room.GetGameMap().ValidTile(newX, newY) || _room.GetGameMap().SquareHasUsers(newX, newY) && !item.GetBaseItem().IsSeat)
            {
                if (needsReAdd)
                    _room.GetGameMap().AddToMap(item);
                return false;
            }

            foreach (ThreeDCoord tile in affectedTiles.Values)
            {
                if (!_room.GetGameMap().ValidTile(tile.X, tile.Y) ||
                    (_room.GetGameMap().SquareHasUsers(tile.X, tile.Y) && !item.GetBaseItem().IsSeat))
                {
                    if (needsReAdd)
                    {
                        _room.GetGameMap().AddToMap(item);
                    }

                    return false;
                }
            }

            // Start calculating new Z coordinate
            double newZ = _room.GetGameMap().Model.SqFloorHeight[newX, newY];

            if (height == -1)
            {
                if (!onRoller)
                {
                    // Make sure this tile is open and there are no users here
                    if (_room.GetGameMap().Model.SqState[newX, newY] != SquareState.Open && !item.GetBaseItem().IsSeat)
                    {
                        return false;
                    }

                    foreach (ThreeDCoord tile in affectedTiles.Values)
                    {
                        if (_room.GetGameMap().Model.SqState[tile.X, tile.Y] != SquareState.Open &&
                            !item.GetBaseItem().IsSeat)
                        {
                            if (needsReAdd)
                            {
                                //AddItem(Item);
                                _room.GetGameMap().AddToMap(item);
                            }

                            return false;
                        }
                    }

                    // And that we have no users
                    if (!item.GetBaseItem().IsSeat && !item.IsRoller)
                    {
                        foreach (ThreeDCoord tile in affectedTiles.Values)
                        {
                            if (_room.GetGameMap().GetRoomUsers(new Point(tile.X, tile.Y)).Count > 0)
                            {
                                if (needsReAdd)
                                    _room.GetGameMap().AddToMap(item);
                                return false;
                            }
                        }
                    }
                }

                // Find affected objects
                var itemsAffected = new List<Item>();
                var itemsComplete = new List<Item>();

                foreach (ThreeDCoord tile in affectedTiles.Values.ToList())
                {
                    List<Item> temp = GetFurniObjects(tile.X, tile.Y);

                    if (temp != null)
                    {
                        itemsAffected.AddRange(temp);
                    }
                }

                itemsComplete.AddRange(itemsOnTile);
                itemsComplete.AddRange(itemsAffected);

                if (!onRoller)
                {
                    // Check for items in the stack that do not allow stacking on top of them
                    foreach (Item I in itemsComplete.ToList())
                    {
                        if (I == null)
                            continue;

                        if (I.Id == item.Id)
                            continue;

                        if (I.GetBaseItem() == null)
                            continue;

                        if (!I.GetBaseItem().Stackable)
                        {
                            if (needsReAdd)
                            {
                                //AddItem(Item);
                                _room.GetGameMap().AddToMap(item);
                            }

                            return false;
                        }
                    }
                }

                //if (!Item.IsRoller)
                {
                    // If this is a rotating action, maintain item at current height
                    if (item.Rotation != newRot && item.GetX == newX && item.GetY == newY)
                        newZ = item.GetZ;

                    // Are there any higher objects in the stack!?
                    foreach (Item i in itemsComplete.ToList())
                    {
                        if (i == null)
                            continue;
                        if (i.Id == item.Id)
                            continue;

                        if (i.GetBaseItem().InteractionType == InteractionType.StackTool)
                        {
                            newZ = i.GetZ;
                            break;
                        }

                        if (i.TotalHeight > newZ)
                        {
                            newZ = i.TotalHeight;
                        }
                    }
                }

            }
            else
                newZ = height;

            item.Rotation = newRot;
            int oldX = item.GetX;
            int oldY = item.GetY;
            item.SetState(newX, newY, newZ, affectedTiles);

            if (!onRoller && session != null)
                item.Interactor.OnPlace(session, item);


            if (newItem)
            {
                if (_floorItems.ContainsKey(item.Id))
                {
                    session?.SendNotification(PlusEnvironment.GetLanguageManager().TryGetValue("room.item.already_placed"));
                    _room.GetGameMap().RemoveFromMap(item);
                    return true;
                }

                if (item.IsFloorItem && !_floorItems.ContainsKey(item.Id))
                    _floorItems.TryAdd(item.Id, item);
                else if (item.IsWallItem && !_wallItems.ContainsKey(item.Id))
                    _wallItems.TryAdd(item.Id, item);

                if (sendMessage)
                    _room.SendPacket(new ObjectAddComposer(item));
            }
            else
            {
                UpdateItem(item);
                if (!onRoller && sendMessage)
                    _room.SendPacket(new ObjectUpdateComposer(item, _room.OwnerId));
            }

            _room.GetGameMap().AddToMap(item);

            if (item.GetBaseItem().IsSeat)
                updateRoomUserStatuses = true;

            if (updateRoomUserStatuses)
                _room.GetRoomUserManager().UpdateUserStatusses();

            if (item.GetBaseItem().InteractionType == InteractionType.Tent || item.GetBaseItem().InteractionType == InteractionType.TentSmall)
            {
                _room.RemoveTent(item.Id);
                _room.AddTent(item.Id);
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `items` SET `room_id` = '" + _room.RoomId + "', `x` = '" + item.GetX + "', `y` = '" + item.GetY + "', `z` = '" + item.GetZ + "', `rot` = '" + item.Rotation + "' WHERE `id` = '" + item.Id + "' LIMIT 1");
            }

            return true;
        }

        private static int NormalizeFloorRotation(int rotation)
        {
            if (rotation < 0)
                return 0;

            return rotation % 8;
        }

        public List<Item> GetFurniObjects(int x, int y)
        {
            return _room.GetGameMap().GetCoordinatedItems(new Point(x, y));
        }

        public bool SetFloorItem(Item item, int newX, int newY, double newZ)
        {
            if (_room == null)
                return false;

            _room.GetGameMap().RemoveFromMap(item);

            item.SetState(newX, newY, newZ, Gamemap.GetAffectedTiles(item.GetBaseItem().Length, item.GetBaseItem().Width, newX, newY, item.Rotation));
            if (item.GetBaseItem().InteractionType == InteractionType.Toner)
            {
                if (_room.TonerData == null)
                {
                    _room.TonerData = new TonerData(item.Id);
                }
            }

            UpdateItem(item);
            _room.GetGameMap().AddItemToMap(item);
            return true;
        }

        public bool SetWallItem(GameClient session, Item item)
        {
            if (!item.IsWallItem || _wallItems.ContainsKey(item.Id))
                return false;

            if (_floorItems.ContainsKey(item.Id))
            {
                session.SendNotification(PlusEnvironment.GetLanguageManager().TryGetValue("room.item.already_placed"));
                return true;
            }

            item.Interactor.OnPlace(session, item);
            if (item.GetBaseItem().InteractionType == InteractionType.Moodlight)
            {
                if (_room.MoodlightData == null)
                {
                    _room.MoodlightData = new MoodlightData(item.Id);
                    item.ExtraData = _room.MoodlightData.GenerateExtraData();
                }
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `items` SET `room_id` = '" + _room.RoomId + "', `x` = '" + item.GetX + "', `y` = '" + item.GetY + "', `z` = '" + item.GetZ + "', `rot` = '" + item.Rotation + "', `wall_pos` = @WallPos WHERE `id` = '" + item.Id + "' LIMIT 1");
                dbClient.AddParameter("WallPos", item.WallCoord);
                dbClient.RunQuery();
            }

            _wallItems.TryAdd(item.Id, item);

            _room.SendPacket(new ItemAddComposer(item));

            return true;
        }

        public void UpdateItem(Item item)
        {
            if (item == null)
                return;
            if (!_movedItems.ContainsKey(item.Id))
                _movedItems.TryAdd(item.Id, item);
        }

        public void RemoveItem(Item item)
        {
            if (item == null)
                return;

            if (_movedItems.ContainsKey(item.Id))
                _movedItems.TryRemove(item.Id, out item);

            if (item != null && _rollers.ContainsKey(item.Id))
                _rollers.TryRemove(item.Id, out item);
        }

        public void OnCycle()
        {
            if (GotRollers)
            {
                try
                {
                    _room.SendPacket(CycleRollers());
                }
                catch //(Exception e)
                {
                    // Logging.LogThreadException(e.ToString(), "rollers for room with ID " + room.RoomId);
                    GotRollers = false;
                }
            }

            if (_roomItemUpdateQueue.Count > 0)
            {
                List<Item> addItems = new();
                while (_roomItemUpdateQueue.Count > 0)
                {
                    if (_roomItemUpdateQueue.TryDequeue(out Item item))
                    {
                        item.ProcessUpdates();

                        if (item.UpdateCounter > 0)
                            addItems.Add(item);
                    }
                }

                foreach (Item item in addItems.ToList())
                {
                    if (item == null)
                        continue;

                    _roomItemUpdateQueue.Enqueue(item);
                }
            }
        }

        public List<Item> RemoveItems(GameClient session)
        {
            List<Item> items = new();

            foreach (Item item in GetWallAndFloor.ToList())
            {
                if (item == null || item.UserId != session.GetHabbo().Id)
                    continue;

                if (item.IsFloorItem)
                {
                    _floorItems.TryRemove(item.Id, out Item I);
                    session.GetHabbo().GetInventoryComponent().TryAddFloorItem(item.Id, I);
                    _room.SendPacket(new ObjectRemoveComposer(item, item.UserId));
                }
                else if (item.IsWallItem)
                {
                    _wallItems.TryRemove(item.Id, out Item I);
                    session.GetHabbo().GetInventoryComponent().TryAddWallItem(item.Id, I);
                    _room.SendPacket(new ItemRemoveComposer(item, item.UserId));
                }

                session.SendPacket(new FurniListAddComposer(item));
            }

            _rollers.Clear();
            return items;
        }

        public ICollection<Item> GetFloor => _floorItems.Values;

        public ICollection<Item> GetWall => _wallItems.Values;

        public IEnumerable<Item> GetWallAndFloor => _floorItems.Values.Concat(_wallItems.Values);

        public bool CheckPosItem(Item item, int newX, int newY, int newRot)
        {
            try
            {
                Dictionary<int, ThreeDCoord> dictionary = Gamemap.GetAffectedTiles(item.GetBaseItem().Length, item.GetBaseItem().Width, newX, newY, newRot);
                if (!_room.GetGameMap().ValidTile(newX, newY))
                    return false;

                foreach (ThreeDCoord coord in dictionary.Values.ToList())
                {
                    if ((_room.GetGameMap().Model.DoorX == coord.X) && (_room.GetGameMap().Model.DoorY == coord.Y))
                        return false;
                }

                if ((_room.GetGameMap().Model.DoorX == newX) && (_room.GetGameMap().Model.DoorY == newY))
                    return false;

                foreach (ThreeDCoord coord in dictionary.Values.ToList())
                {
                    if (!_room.GetGameMap().ValidTile(coord.X, coord.Y))
                        return false;
                }

                double num = _room.GetGameMap().Model.SqFloorHeight[newX, newY];
                if ((((item.Rotation == newRot) && (item.GetX == newX)) && (item.GetY == newY)) && (item.GetZ != num))
                    return false;

                if (_room.GetGameMap().Model.SqState[newX, newY] != SquareState.Open)
                    return false;

                foreach (ThreeDCoord coord in dictionary.Values.ToList())
                {
                    if (_room.GetGameMap().Model.SqState[coord.X, coord.Y] != SquareState.Open)
                        return false;
                }

                if (!item.GetBaseItem().IsSeat)
                {
                    if (_room.GetGameMap().SquareHasUsers(newX, newY))
                        return false;

                    foreach (ThreeDCoord coord in dictionary.Values.ToList())
                    {
                        if (_room.GetGameMap().SquareHasUsers(coord.X, coord.Y))
                            return false;
                    }
                }

                List<Item> furniObjects = GetFurniObjects(newX, newY);
                List<Item> collection = new();
                List<Item> list3 = new();
                foreach (ThreeDCoord coord in dictionary.Values.ToList())
                {
                    List<Item> list4 = GetFurniObjects(coord.X, coord.Y);
                    if (list4 != null)
                        collection.AddRange(list4);
                }

                if (furniObjects == null)
                    furniObjects = new List<Item>();

                list3.AddRange(furniObjects);
                list3.AddRange(collection);
                foreach (Item i in list3.ToList())
                {
                    if ((i.Id != item.Id) && !i.GetBaseItem().Stackable)
                        return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public ICollection<Item> GetRollers()
        {
            return _rollers.Values;
        }

        public void Dispose()
        {
            SaveFurniture();

            foreach (Item item in GetWallAndFloor.ToList())
            {
                item?.Destroy();
            }

            _movedItems.Clear();
            _rollers.Clear();
            _wallItems.Clear();
            _floorItems.Clear();
            _rollerItemsMoved.Clear();
            _rollerUsersMoved.Clear();
            _rollerMessages.Clear();
            _roomItemUpdateQueue = null;
        }
    }
}
