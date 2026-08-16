using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Core;
using Plus.Database.EF;
using Microsoft.EntityFrameworkCore;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Item = Plus.HabboHotel.Items.Item;
using Plus.HabboHotel.Items.Data.Moodlight;
using Plus.HabboHotel.Items.Data.Toner;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Rooms.PathFinding;

namespace Plus.HabboHotel.Rooms
{
    public class RoomItemHandling(Room room)
    {
        public int HopperCount = 0;
        private int _mRollerSpeed = 4;
        private int _mRollerCycle = 0;

        private readonly ConcurrentDictionary<int, Item> _movedItems = new();

        private readonly ConcurrentDictionary<int, Item> _rollers = new();
        private readonly ConcurrentDictionary<int, Item> _wallItems = new();
        private readonly ConcurrentDictionary<int, Item> _floorItems = new();

        private readonly List<int> _rollerItemsMoved = [];
        private readonly List<int> _rollerUsersMoved = [];
        private readonly List<MessageComposer> _rollerMessages = [];

        private ConcurrentQueue<Item> _roomItemUpdateQueue = new();

        private Item[] _pressurePlates = [];
        private volatile bool _pressurePlatesDirty = true;

        public Item[] GetPressurePlates()
        {
            if (!_pressurePlatesDirty)
                return _pressurePlates;

            // Cleared first: a placement racing this rebuild has to leave the flag set, not lose it.
            _pressurePlatesDirty = false;

            List<Item> plates = null;

            foreach (Item item in _floorItems.Values) {
                if (item?.GetBaseItem()?.InteractionType == InteractionType.PressurePlate)
                    (plates ??= []).Add(item);
            }

            _pressurePlates = plates == null ? [] : [.. plates];

            return _pressurePlates;
        }

        /// <summary>Call whenever the floor item set changes.</summary>
        public void InvalidatePressurePlates()
        {
            _pressurePlatesDirty = true;
        }

        public void TryAddRoller(int itemId, Item roller)
        {
            _rollers.TryAdd(itemId, roller);
        }

        public bool GotRollers { get; set; } = false;

        public void QueueRoomItemUpdate(Item item)
        {
            _roomItemUpdateQueue.Enqueue(item);
        }

        public void SetSpeed(int p)
        {
            _mRollerSpeed = p;
        }

        public static string WallPositionCheck(string wallPosition)
        {
            //:w=3,2 l=9,63 l
            try {
                if (wallPosition.Contains(Convert.ToChar(13))) {
                    return null;
                }

                if (wallPosition.Contains(Convert.ToChar(9))) {
                    return null;
                }

                string[] posD = wallPosition.Split(' ');
                if (posD[2] != "l" && posD[2] != "r")
                    return null;

                string[] widD = posD[0][3..].Split(',');
                int widthX = int.Parse(widD[0]);
                int widthY = int.Parse(widD[1]);
                if (widthX < -1000 || widthY < -1 || widthX > 700 || widthY > 700)
                    return null;

                string[] lenD = posD[1][2..].Split(',');
                int lengthX = int.Parse(lenD[0]);
                int lengthY = int.Parse(lenD[1]);
                if (lengthX < -1 || lengthY < -1000 || lengthX > 700 || lengthY > 700)
                    return null;

                return ":w=" + widthX + "," + widthY + " " + "l=" + lengthX + "," + lengthY + " " + posD[2];
            } catch {
                return null;
            }
        }

        public void LoadFurniture()
        {
            InvalidatePressurePlates();

            if (!_floorItems.IsEmpty)
                _floorItems.Clear();
            if (!_wallItems.IsEmpty)
                _wallItems.Clear();

            // Re-assign orphaned items to the room owner in a single statement instead of one
            // UPDATE (and one borrowed DB connection) per item during room load.
            uint roomIdKey = (uint)room.Id;
            int ownerId = room.OwnerId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Items.Where(i => i.RoomId == roomIdKey && i.UserId == 0)
                    .ExecuteUpdate(s => s.SetProperty(i => i.UserId, ownerId));
            }

            List<Item> items = ItemLoader.GetItemsForRoom(room.Id, room);
            foreach (Item item in items) {
                if (item == null)
                    continue;

                if (item.IsFloorItem) {
                    if (!room.GetGameMap().ValidTile(item.GetX, item.GetY)) {
                        uint orphanId = (uint)item.Id;
                        using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                            db.Items.Where(i => i.Id == orphanId).ExecuteUpdate(s => s.SetProperty(i => i.RoomId, (uint)0));
                        }

                        GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(item.UserId);
                        if (client != null) {
                            client.GetHabbo().GetInventoryComponent().AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true, item.LimitedNo, item.LimitedTot);
                            client.GetHabbo().GetInventoryComponent().UpdateItems(false);
                        }

                        continue;
                    }

                    if (!_floorItems.ContainsKey(item.Id))
                        _floorItems.TryAdd(item.Id, item);
                } else if (item.IsWallItem) {
                    if (string.IsNullOrWhiteSpace(item.WallCoord)) {
                        uint wallItemId = (uint)item.Id;
                        using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                            db.Items.Where(i => i.Id == wallItemId).ExecuteUpdate(s => s.SetProperty(i => i.WallPos, ":w=0,2 l=11,53 l"));
                        }

                        item.WallCoord = ":w=0,2 l=11,53 l";
                    }

                    try {
                        item.WallCoord = WallPositionCheck(":" + item.WallCoord.Split(':')[1]);
                    } catch {
                        uint wallItemId = (uint)item.Id;
                        using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                            db.Items.Where(i => i.Id == wallItemId).ExecuteUpdate(s => s.SetProperty(i => i.WallPos, ":w=0,2 l=11,53 l"));
                        }

                        item.WallCoord = ":w=0,2 l=11,53 l";
                    }

                    if (!_wallItems.ContainsKey(item.Id))
                        _wallItems.TryAdd(item.Id, item);
                }
            }

            foreach (Item item in _floorItems.Values.ToList()) {
                if (item.IsRoller) {
                    GotRollers = true;
                } else if (item.GetBaseItem().InteractionType == InteractionType.Moodlight) {
                    room.MoodlightData ??= new MoodlightData(item.Id);
                } else if (item.GetBaseItem().InteractionType == InteractionType.Toner) {
                    room.TonerData ??= new TonerData(item.Id);
                } else if (item.IsWired) {
                    if (room?.GetWired() == null)
                        continue;

                    room.GetWired().LoadWiredBox(item);
                } else if (item.GetBaseItem().InteractionType == InteractionType.Hopper)
                    HopperCount++;
            }
        }

        public Item GetItem(int pId)
        {
            if (_floorItems != null && _floorItems.ContainsKey(pId)) {
                if (_floorItems.TryGetValue(pId, out Item item))
                    return item;
            } else if (_wallItems != null && _wallItems.ContainsKey(pId)) {
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
                room.GetSoccer().UnRegisterGate(item);

            if (item.GetBaseItem().InteractionType != InteractionType.Gift)
                item.Interactor.OnRemove(session, item);

            if (item.GetBaseItem().InteractionType == InteractionType.GuildGate) {
                item.UpdateCounter = 0;
                item.UpdateNeeded = false;
            }

            RemoveRoomItem(item);
        }

        private void RemoveRoomItem(Item item)
        {
            InvalidatePressurePlates();

            if (item.IsFloorItem)
                room.SendPacket(new ObjectRemoveComposer(item, item.UserId));
            else if (item.IsWallItem)
                room.SendPacket(new ItemRemoveComposer(item, item.UserId));

            //TODO: Recode this specific part
            if (item.IsWallItem)
                _wallItems.TryRemove(item.Id, out item);
            else {
                _floorItems.TryRemove(item.Id, out item);
                //mFloorItems.OnCycle();
                room.GetGameMap().RemoveFromMap(item);
            }

            RemoveItem(item);
            room.GetGameMap().GenerateMaps();
            room.GetRoomUserManager().UpdateUserStatusses();
        }

        private List<MessageComposer> CycleRollers()
        {
            if (!GotRollers)
                return [];

            if (_mRollerCycle >= _mRollerSpeed || _mRollerSpeed == 0) {
                _rollerItemsMoved.Clear();
                _rollerUsersMoved.Clear();
                _rollerMessages.Clear();

                foreach (Item roller in _rollers.Values.ToList()) {
                    if (roller == null)
                        continue;

                    Point nextSquare = roller.SquareInFront;

                    List<Item> itemsOnRoller = room.GetGameMap().GetRoomItemForSquare(roller.GetX, roller.GetY, roller.GetZ);
                    List<Item> itemsOnNext = [.. room.GetGameMap().GetAllRoomItemForSquare(nextSquare.X, nextSquare.Y)];

                    if (itemsOnRoller.Count > 10)
                        itemsOnRoller = [.. room.GetGameMap().GetRoomItemForSquare(roller.GetX, roller.GetY, roller.GetZ).Take(10)];

                    bool nextSquareIsRoller = (itemsOnNext.Any(x => x.GetBaseItem().InteractionType == InteractionType.Roller));
                    bool nextRollerClear = true;

                    double nextZ = 0.0;
                    bool nextRoller = false;

                    foreach (Item item in itemsOnNext.ToList()) {
                        if (item.IsRoller) {
                            if (item.TotalHeight > nextZ)
                                nextZ = item.TotalHeight;

                            nextRoller = true;
                        }
                    }

                    if (nextRoller) {
                        foreach (Item item in itemsOnNext.ToList()) {
                            if (item.TotalHeight > nextZ)
                                nextRollerClear = false;
                        }
                    }

                    if (itemsOnRoller.Count > 0) {
                        foreach (Item rItem in itemsOnRoller.ToList()) {
                            if (rItem == null)
                                continue;

                            if (!_rollerItemsMoved.Contains(rItem.Id) && room.GetGameMap().CanRollItemHere(nextSquare.X, nextSquare.Y) && nextRollerClear && roller.GetZ < rItem.GetZ && room.GetRoomUserManager().GetUserForSquare(nextSquare.X, nextSquare.Y) == null) {
                                if (!nextSquareIsRoller)
                                    nextZ = rItem.GetZ - roller.EffectiveHeight;
                                else
                                    nextZ = rItem.GetZ;

                                _rollerMessages.Add(UpdateItemOnRoller(rItem, nextSquare, roller.Id, nextZ));
                                _rollerItemsMoved.Add(rItem.Id);
                            }
                        }
                    }

                    RoomUser rollerUser = room.GetGameMap().GetRoomUsers(roller.Coordinate).FirstOrDefault();

                    if (rollerUser != null && !rollerUser.IsWalking && nextRollerClear && room.GetGameMap().IsValidStep(new Vector2D(roller.GetX, roller.GetY), new Vector2D(nextSquare.X, nextSquare.Y), true, false, true) && room.GetGameMap().CanRollItemHere(nextSquare.X, nextSquare.Y) && room.GetGameMap().GetFloorStatus(nextSquare) != 0) {
                        if (!_rollerUsersMoved.Contains(rollerUser.HabboId)) {
                            if (!nextSquareIsRoller)
                                nextZ = rollerUser.Z - roller.EffectiveHeight;
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

            return [];
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

            room.GetGameMap()
                .UpdateUserMovement(new Point(pUser.X, pUser.Y), new Point(pNextCoord.X, pNextCoord.Y), pUser);
            room.GetGameMap().GameMap[pUser.X, pUser.Y] = 1;
            pUser.X = pNextCoord.X;
            pUser.Y = pNextCoord.Y;
            pUser.Z = nextZ;

            room.GetGameMap().GameMap[pUser.X, pUser.Y] = 0;

            // Pressure plates need no help here: rollers skip the walk-on/walk-off hooks, but the
            // movement tick reads positions rather than hooks and picks this up on its own.

            if (pUser != null && pUser.GetClient() != null && pUser.GetClient().GetHabbo() != null) {
                List<Item> items = room.GetGameMap().GetRoomItemForSquare(pNextCoord.X, pNextCoord.Y);
                foreach (Item IItem in items.ToList()) {
                    if (IItem == null)
                        continue;

                    room.GetWired().TriggerEvent(WiredBoxType.TriggerWalkOnFurni, pUser.GetClient().GetHabbo(), IItem);
                }

                Item item = room.GetRoomItemHandler().GetItem(pRollerId);
                if (item != null) {
                    room.GetWired().TriggerEvent(WiredBoxType.TriggerWalkOffFurni, pUser.GetClient().GetHabbo(), item);
                }
            }

            return mMessage;
        }

        public void FlushMovedItems()
        {
            SaveFurniture();
        }

        private void SaveFurniture()
        {
            try {
                if (!_movedItems.IsEmpty) {
                    using WavePlusContext db = PlusEnvironment.GetDbContext();
                    foreach (Item item in _movedItems.Values.ToList()) {
                        uint itemId = (uint)item.Id;

                        if (!string.IsNullOrEmpty(item.ExtraData)) {
                            string edata = item.ExtraData;
                            db.Items.Where(i => i.Id == itemId).ExecuteUpdate(s => s.SetProperty(i => i.ExtraData, edata));
                        }

                        if (item.IsWallItem && (!item.GetBaseItem().ItemName.Contains("wallpaper_single") || !item.GetBaseItem().ItemName.Contains("floor_single") || !item.GetBaseItem().ItemName.Contains("landscape_single"))) {
                            string wallPos = item.WallCoord;
                            db.Items.Where(i => i.Id == itemId).ExecuteUpdate(s => s.SetProperty(i => i.WallPos, wallPos));
                        }

                        int x = item.GetX, y = item.GetY, rot = item.Rotation;
                        double z = item.GetZ;
                        db.Items.Where(i => i.Id == itemId).ExecuteUpdate(s => s
                            .SetProperty(i => i.X, x)
                            .SetProperty(i => i.Y, y)
                            .SetProperty(i => i.Z, z)
                            .SetProperty(i => i.Rot, rot));
                    }
                }
            } catch (Exception e) {
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

            List<Point> previousFootprint = newItem ? [] : item.GetCoords;

            if (newItem) {
                if (item.IsWired) {
                    if (item.GetBaseItem().WiredType == WiredBoxType.EffectRegenerateMaps && room.GetRoomItemHandler().GetFloor.Any(x => x.GetBaseItem().WiredType == WiredBoxType.EffectRegenerateMaps))
                        return false;
                }
            }

            List<Item> itemsOnTile = GetFurniObjects(newX, newY);
            if (item.GetBaseItem().InteractionType == InteractionType.Roller && itemsOnTile.Any(x => x.GetBaseItem().InteractionType == InteractionType.Roller && x.Id != item.Id))
                return false;

            if (!newItem)
                needsReAdd = room.GetGameMap().RemoveFromMap(item);

            Dictionary<int, ThreeDCoord> affectedTiles = Gamemap.GetAffectedTiles(item.GetBaseItem().Length, item.GetBaseItem().Width, newX, newY, newRot);

            Item stackHelper = null;
            {
                List<Item> helperCheck = GetFurniObjects(newX, newY) ?? [];
                foreach (ThreeDCoord tile in affectedTiles.Values) {
                    List<Item> t = GetFurniObjects(tile.X, tile.Y);
                    if (t != null)
                        helperCheck.AddRange(t);
                }

                foreach (Item ci in helperCheck) {
                    if (ci != null && ci.Id != item.Id && ci.GetBaseItem() != null
                        && ci.GetBaseItem().InteractionType == InteractionType.StackTool) {
                        if (stackHelper == null || ci.GetZ < stackHelper.GetZ)
                            stackHelper = ci;
                    }
                }
            }
            bool hasStackHelper = stackHelper != null;

            if (!room.GetGameMap().ValidTile(newX, newY)) {
                if (needsReAdd)
                    room.GetGameMap().AddToMap(item);
                return false;
            }

            foreach (ThreeDCoord tile in affectedTiles.Values) {
                if (!room.GetGameMap().ValidTile(tile.X, tile.Y)) {
                    if (needsReAdd) {
                        room.GetGameMap().AddToMap(item);
                    }

                    return false;
                }
            }

            // Start calculating new Z coordinate
            double newZ = room.GetGameMap().Model.SqFloorHeight[newX, newY];

            if (hasStackHelper) {
                // Magic override: ignore stack config, take the helper's height directly
                // (including 0, which drops the item under everything else).
                newZ = stackHelper.GetZ;
            } else if (height == -1) {
                if (!onRoller) {
                    // Make sure this tile is open and there are no users here
                    if (room.GetGameMap().Model.SqState[newX, newY] != SquareState.Open && !item.GetBaseItem().IsSeat) {
                        return false;
                    }

                    foreach (ThreeDCoord tile in affectedTiles.Values) {
                        if (room.GetGameMap().Model.SqState[tile.X, tile.Y] != SquareState.Open &&
                            !item.GetBaseItem().IsSeat) {
                            if (needsReAdd) {
                                //AddItem(Item);
                                room.GetGameMap().AddToMap(item);
                            }

                            return false;
                        }
                    }

                }

                // Find affected objects
                var itemsAffected = new List<Item>();
                var itemsComplete = new List<Item>();

                foreach (ThreeDCoord tile in affectedTiles.Values.ToList()) {
                    List<Item> temp = GetFurniObjects(tile.X, tile.Y);

                    if (temp != null) {
                        itemsAffected.AddRange(temp);
                    }
                }

                itemsComplete.AddRange(itemsOnTile);
                itemsComplete.AddRange(itemsAffected);

                if (!onRoller) {
                    // Check for items in the stack that do not allow stacking on top of them
                    foreach (Item I in itemsComplete.ToList()) {
                        if (I == null)
                            continue;

                        if (I.Id == item.Id)
                            continue;

                        if (I.GetBaseItem() == null)
                            continue;

                        if (!I.GetBaseItem().Stackable) {
                            if (needsReAdd) {
                                //AddItem(Item);
                                room.GetGameMap().AddToMap(item);
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
                    foreach (Item i in itemsComplete.ToList()) {
                        if (i == null)
                            continue;
                        if (i.Id == item.Id)
                            continue;

                        if (i.GetBaseItem().InteractionType == InteractionType.StackTool) {
                            newZ = i.GetZ;
                            break;
                        }

                        if (i.TotalHeight > newZ) {
                            newZ = i.TotalHeight;
                        }
                    }
                }

            } else
                newZ = height;

            item.Rotation = newRot;
            int oldX = item.GetX;
            int oldY = item.GetY;
            item.SetState(newX, newY, newZ, affectedTiles);

            if (!onRoller && session != null)
                item.Interactor.OnPlace(session, item);

            if (newItem) {
                if (_floorItems.ContainsKey(item.Id)) {
                    session?.SendNotification(PlusEnvironment.GetLanguageManager().TryGetValue("room.item.already_placed"));
                    room.GetGameMap().RemoveFromMap(item);
                    return true;
                }

                if (item.IsFloorItem && !_floorItems.ContainsKey(item.Id)) {
                    _floorItems.TryAdd(item.Id, item);
                    InvalidatePressurePlates();
                } else if (item.IsWallItem && !_wallItems.ContainsKey(item.Id))
                    _wallItems.TryAdd(item.Id, item);

                if (sendMessage)
                    room.SendPacket(new ObjectAddComposer(item));
            } else {
                UpdateItem(item);
                if (!onRoller && sendMessage)
                    room.SendPacket(new ObjectUpdateComposer(item, room.OwnerId));
            }

            room.GetGameMap().AddToMap(item);

            if (item.GetBaseItem().IsSeat)
                updateRoomUserStatuses = true;

            if (room.GetGameMap().SquareHasUsers(newX, newY) || room.GetGameMap().SquareHasUsers(oldX, oldY))
                updateRoomUserStatuses = true;

            foreach (ThreeDCoord tile in affectedTiles.Values) {
                if (room.GetGameMap().SquareHasUsers(tile.X, tile.Y)) {
                    updateRoomUserStatuses = true;
                    break;
                }
            }

            foreach (Point tile in previousFootprint) {
                if (room.GetGameMap().SquareHasUsers(tile.X, tile.Y)) {
                    updateRoomUserStatuses = true;
                    break;
                }
            }

            if (updateRoomUserStatuses)
                room.GetRoomUserManager().UpdateUserStatusses();

            if (item.GetBaseItem().InteractionType == InteractionType.Tent || item.GetBaseItem().InteractionType == InteractionType.TentSmall) {
                room.RemoveTent(item.Id);
                room.AddTent(item.Id);
            }

            uint floorItemId = (uint)item.Id;
            uint floorRoomId = (uint)room.RoomId;
            int fx = item.GetX, fy = item.GetY, frot = item.Rotation;
            double fz = item.GetZ;
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            db.Items.Where(i => i.Id == floorItemId).ExecuteUpdate(s => s
                .SetProperty(i => i.RoomId, floorRoomId)
                .SetProperty(i => i.X, fx)
                .SetProperty(i => i.Y, fy)
                .SetProperty(i => i.Z, fz)
                .SetProperty(i => i.Rot, frot));

            return true;
        }

        private static int NormalizeFloorRotation(int rotation)
        {
            if (rotation < 0)
                return 0;

            return rotation % 8;
        }

        public List<Item> GetFurniObjects(int x, int y) => room.GetGameMap().GetCoordinatedItems(new Point(x, y));

        public bool SetFloorItem(Item item, int newX, int newY, double newZ)
        {
            if (room == null)
                return false;

            room.GetGameMap().RemoveFromMap(item);

            item.SetState(newX, newY, newZ, Gamemap.GetAffectedTiles(item.GetBaseItem().Length, item.GetBaseItem().Width, newX, newY, item.Rotation));
            if (item.GetBaseItem().InteractionType == InteractionType.Toner) {
                room.TonerData ??= new TonerData(item.Id);
            }

            UpdateItem(item);
            room.GetGameMap().AddItemToMap(item);
            return true;
        }

        public bool SetWallItem(GameClient session, Item item)
        {
            if (!item.IsWallItem || _wallItems.ContainsKey(item.Id))
                return false;

            if (_floorItems.ContainsKey(item.Id)) {
                session.SendNotification(PlusEnvironment.GetLanguageManager().TryGetValue("room.item.already_placed"));
                return true;
            }

            item.Interactor.OnPlace(session, item);
            if (item.GetBaseItem().InteractionType == InteractionType.Moodlight) {
                if (room.MoodlightData == null) {
                    room.MoodlightData = new MoodlightData(item.Id);
                    item.ExtraData = room.MoodlightData.GenerateExtraData();
                }
            }

            uint wallSaveId = (uint)item.Id;
            uint wallRoomId = (uint)room.RoomId;
            int wx = item.GetX, wy = item.GetY, wrot = item.Rotation;
            double wz = item.GetZ;
            string wallCoord = item.WallCoord;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Items.Where(i => i.Id == wallSaveId).ExecuteUpdate(s => s
                    .SetProperty(i => i.RoomId, wallRoomId)
                    .SetProperty(i => i.X, wx)
                    .SetProperty(i => i.Y, wy)
                    .SetProperty(i => i.Z, wz)
                    .SetProperty(i => i.Rot, wrot)
                    .SetProperty(i => i.WallPos, wallCoord));
            }

            _wallItems.TryAdd(item.Id, item);

            room.SendPacket(new ItemAddComposer(item));

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
                _rollers.TryRemove(item.Id, out _);
        }

        public void OnCycle()
        {
            if (GotRollers) {
                try {
                    room.SendPacket(CycleRollers());
                } catch //(Exception e)
                  {
                    // Logging.LogThreadException(e.ToString(), "rollers for room with ID " + room.RoomId);
                    GotRollers = false;
                }
            }

            if (!_roomItemUpdateQueue.IsEmpty) {
                List<Item> addItems = [];
                while (!_roomItemUpdateQueue.IsEmpty) {
                    if (_roomItemUpdateQueue.TryDequeue(out Item item)) {
                        item.ProcessUpdates();

                        if (item.UpdateCounter > 0)
                            addItems.Add(item);
                    }
                }

                foreach (Item item in addItems.ToList()) {
                    if (item == null)
                        continue;

                    _roomItemUpdateQueue.Enqueue(item);
                }
            }
        }

        public List<Item> RemoveItems(GameClient session)
        {
            InvalidatePressurePlates();

            List<Item> items = [];

            foreach (Item item in GetWallAndFloor.ToList()) {
                if (item == null || item.UserId != session.GetHabbo().Id)
                    continue;

                if (item.IsFloorItem) {
                    _floorItems.TryRemove(item.Id, out Item I);
                    session.GetHabbo().GetInventoryComponent().TryAddFloorItem(item.Id, I);
                    room.SendPacket(new ObjectRemoveComposer(item, item.UserId));
                } else if (item.IsWallItem) {
                    _wallItems.TryRemove(item.Id, out Item I);
                    session.GetHabbo().GetInventoryComponent().TryAddWallItem(item.Id, I);
                    room.SendPacket(new ItemRemoveComposer(item, item.UserId));
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
            try {
                Dictionary<int, ThreeDCoord> dictionary = Gamemap.GetAffectedTiles(item.GetBaseItem().Length, item.GetBaseItem().Width, newX, newY, newRot);
                if (!room.GetGameMap().ValidTile(newX, newY))
                    return false;

                foreach (ThreeDCoord coord in dictionary.Values.ToList()) {
                    if ((room.GetGameMap().Model.DoorX == coord.X) && (room.GetGameMap().Model.DoorY == coord.Y))
                        return false;
                }

                if ((room.GetGameMap().Model.DoorX == newX) && (room.GetGameMap().Model.DoorY == newY))
                    return false;

                foreach (ThreeDCoord coord in dictionary.Values.ToList()) {
                    if (!room.GetGameMap().ValidTile(coord.X, coord.Y))
                        return false;
                }

                double num = room.GetGameMap().Model.SqFloorHeight[newX, newY];
                if ((((item.Rotation == newRot) && (item.GetX == newX)) && (item.GetY == newY)) && (item.GetZ != num))
                    return false;

                if (room.GetGameMap().Model.SqState[newX, newY] != SquareState.Open)
                    return false;

                foreach (ThreeDCoord coord in dictionary.Values.ToList()) {
                    if (room.GetGameMap().Model.SqState[coord.X, coord.Y] != SquareState.Open)
                        return false;
                }

                if (!item.GetBaseItem().IsSeat) {
                    if (room.GetGameMap().SquareHasUsers(newX, newY))
                        return false;

                    foreach (ThreeDCoord coord in dictionary.Values.ToList()) {
                        if (room.GetGameMap().SquareHasUsers(coord.X, coord.Y))
                            return false;
                    }
                }

                List<Item> furniObjects = GetFurniObjects(newX, newY);
                List<Item> collection = [];
                List<Item> list3 = [];
                foreach (ThreeDCoord coord in dictionary.Values.ToList()) {
                    List<Item> list4 = GetFurniObjects(coord.X, coord.Y);
                    if (list4 != null)
                        collection.AddRange(list4);
                }

                furniObjects ??= [];

                list3.AddRange(furniObjects);
                list3.AddRange(collection);
                foreach (Item i in list3.ToList()) {
                    if ((i.Id != item.Id) && !i.GetBaseItem().Stackable)
                        return false;
                }

                return true;
            } catch {
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

            foreach (Item item in GetWallAndFloor.ToList()) {
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