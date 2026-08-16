using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Plus.Core;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms.Games.Teams;
using Plus.HabboHotel.Rooms.PathFinding;

namespace Plus.HabboHotel.Rooms
{
    public class Gamemap
    {
        private Room _room;

        public bool DiagonalEnabled { get; set; }
        private double[,] _itemHeightMap;
        private ConcurrentDictionary<Point, List<int>> _coordinatedItems;
        private ConcurrentDictionary<Point, List<RoomUser>> _userMap;

        // 3D view of the room used by the pathfinder. Rebuilt on the room thread whenever
        // furniture moves and then treated as immutable, which is what lets a path be searched
        // off-thread while the user is already walking.
        private RoomHeightMap _heightMap;
        private volatile bool _heightMapDirty = true;
        private readonly object _heightMapLock = new();
        private readonly HashSet<int> _heightMapDirtyTiles = new();
        // Mirrors _heightMapDirtyTiles.Count so the room thread can test for pending work without
        // taking the lock; item placement runs on IO threads, which is what writes that set.
        private volatile int _heightMapDirtyTileCount;

        public Gamemap(Room room, RoomModel model)
        {
            _room = room;
            StaticModel = model;
            DiagonalEnabled = true;

            Model = new DynamicRoomModel(StaticModel);
            _coordinatedItems = new ConcurrentDictionary<Point, List<int>>();
            _itemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];
            _userMap = new ConcurrentDictionary<Point, List<RoomUser>>();
        }

        public void AddUserToMap(RoomUser user, Point coord)
        {
            // The list value is shared between the room-cycle thread (which mutates it during
            // movement) and IO threads (which read it via SquareHasUsers/GetUserForSquare), so
            // every access to the inner list must be locked to avoid intermittent dropped clicks.
            List<RoomUser> users = _userMap.GetOrAdd(coord, _ => new List<RoomUser>());
            lock (users) {
                if (!users.Contains(user))
                    users.Add(user);
            }
        }

        public void TeleportToItem(RoomUser user, Items.Item item)
        {
            if (item == null || user == null)
                return;

            GameMap[user.X, user.Y] = user.SqState;
            UpdateUserMovement(new Point(user.Coordinate.X, user.Coordinate.Y), new Point(item.Coordinate.X, item.Coordinate.Y), user);
            user.X = item.GetX;
            user.Y = item.GetY;
            user.Z = item.GetZ;

            user.SqState = GameMap[item.GetX, item.GetY];
            GameMap[user.X, user.Y] = 1;
            user.RotBody = item.Rotation;
            user.RotHead = item.Rotation;

            user.GoalX = user.X;
            user.GoalY = user.Y;
            user.SetStep = false;
            user.IsWalking = false;
            user.UpdateNeeded = true;
        }

        public void UpdateUserMovement(Point oldCoord, Point newCoord, RoomUser user)
        {
            RemoveUserFromMap(user, oldCoord);
            AddUserToMap(user, newCoord);
        }

        public void RemoveUserFromMap(RoomUser user, Point coord)
        {
            if (_userMap.TryGetValue(coord, out List<RoomUser> users)) {
                lock (users)
                    users.RemoveAll(x => x != null && x.VirtualId == user.VirtualId);
            }
        }

        public bool MapGotUser(Point coord)
        {
            if (_userMap.TryGetValue(coord, out List<RoomUser> users)) {
                lock (users)
                    return users.Count > 0;
            }

            return false;
        }

        public RoomUser GetFirstUserForSquare(Point coord)
        {
            if (_userMap.TryGetValue(coord, out List<RoomUser> users)) {
                lock (users)
                    return users.Count > 0 ? users[0] : null;
            }

            return null;
        }

        public List<RoomUser> GetRoomUsers(Point coord)
        {
            // Return a snapshot so callers on IO threads can enumerate/index safely while the
            // cycle thread mutates the underlying list.
            if (_userMap.TryGetValue(coord, out List<RoomUser> users)) {
                lock (users)
                    return new List<RoomUser>(users);
            }

            return new List<RoomUser>();
        }

        public Point GetRandomWalkableSquare()
        {
            var walkableSquares = new List<Point>();
            for (int y = 0; y < GameMap.GetUpperBound(1); y++) {
                for (int x = 0; x < GameMap.GetUpperBound(0); x++) {
                    if (StaticModel.DoorX != x && StaticModel.DoorY != y && GameMap[x, y] == 1)
                        walkableSquares.Add(new Point(x, y));
                }
            }

            int randomNumber = PlusEnvironment.GetRandomNumber(0, walkableSquares.Count);
            int i = 0;

            foreach (Point coord in walkableSquares.ToList()) {
                if (i == randomNumber)
                    return coord;
                i++;
            }

            return new Point(0, 0);
        }

        public bool IsInMap(int X, int Y)
        {
            var walkableSquares = new List<Point>();
            for (int y = 0; y < GameMap.GetUpperBound(1); y++) {
                for (int x = 0; x < GameMap.GetUpperBound(0); x++) {
                    if (StaticModel.DoorX != x && StaticModel.DoorY != y && GameMap[x, y] == 1)
                        walkableSquares.Add(new Point(x, y));
                }
            }

            if (walkableSquares.Contains(new Point(X, Y)))
                return true;
            return false;
        }

        public void AddToMap(Item item)
        {
            AddItemToMap(item);
        }

        private void SetDefaultValue(int x, int y)
        {
            GameMap[x, y] = 0;
            EffectMap[x, y] = 0;
            _itemHeightMap[x, y] = 0.0;

            if (x == Model.DoorX && y == Model.DoorY) {
                GameMap[x, y] = 3;
            } else if (Model.SqState[x, y] == SquareState.Open) {
                GameMap[x, y] = 1;
            } else if (Model.SqState[x, y] == SquareState.Seat) {
                GameMap[x, y] = 2;
            }
        }

        public void UpdateMapForItem(Item item)
        {
            RemoveFromMap(item);
            AddToMap(item);
        }

        public void GenerateMaps(bool checkLines = true)
        {
            int maxX = 0;
            int maxY = 0;
            _coordinatedItems = new ConcurrentDictionary<Point, List<int>>();

            if (checkLines) {
                Item[] items = _room.GetRoomItemHandler().GetFloor.ToArray();
                foreach (Item item in items.ToList()) {
                    if (item == null)
                        continue;

                    if (item.GetX > Model.MapSizeX && item.GetX > maxX)
                        maxX = item.GetX;
                    if (item.GetY > Model.MapSizeY && item.GetY > maxY)
                        maxY = item.GetY;
                }

                Array.Clear(items, 0, items.Length);
            }

            if (maxY > (Model.MapSizeY - 1) || maxX > (Model.MapSizeX - 1)) {
                if (maxX < Model.MapSizeX)
                    maxX = Model.MapSizeX;
                if (maxY < Model.MapSizeY)
                    maxY = Model.MapSizeY;

                Model.SetMapSize(maxX + 7, maxY + 7);
                GenerateMaps(false);
                return;
            }

            if (maxX != StaticModel.MapSizeX || maxY != StaticModel.MapSizeY) {
                EffectMap = new byte[Model.MapSizeX, Model.MapSizeY];
                GameMap = new byte[Model.MapSizeX, Model.MapSizeY];

                _itemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];
                //if (modelRemap)
                //    Model.Generate(); //Clears model

                for (int line = 0; line < Model.MapSizeY; line++) {
                    for (int chr = 0; chr < Model.MapSizeX; chr++) {
                        GameMap[chr, line] = 0;
                        EffectMap[chr, line] = 0;

                        if (chr == Model.DoorX && line == Model.DoorY) {
                            GameMap[chr, line] = 3;
                        } else if (Model.SqState[chr, line] == SquareState.Open) {
                            GameMap[chr, line] = 1;
                        } else if (Model.SqState[chr, line] == SquareState.Seat) {
                            GameMap[chr, line] = 2;
                        } else if (Model.SqState[chr, line] == SquareState.Pool) {
                            EffectMap[chr, line] = 6;
                        }
                    }
                }
            } else {
                EffectMap = new byte[Model.MapSizeX, Model.MapSizeY];
                GameMap = new byte[Model.MapSizeX, Model.MapSizeY];

                _itemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];

                for (int line = 0; line < Model.MapSizeY; line++) {
                    for (int chr = 0; chr < Model.MapSizeX; chr++) {
                        GameMap[chr, line] = 0;
                        EffectMap[chr, line] = 0;

                        if (chr == Model.DoorX && line == Model.DoorY) {
                            GameMap[chr, line] = 3;
                        } else if (Model.SqState[chr, line] == SquareState.Open) {
                            GameMap[chr, line] = 1;
                        } else if (Model.SqState[chr, line] == SquareState.Seat) {
                            GameMap[chr, line] = 2;
                        } else if (Model.SqState[chr, line] == SquareState.Pool) {
                            EffectMap[chr, line] = 6;
                        }
                    }
                }
            }

            Item[] tmpItems = _room.GetRoomItemHandler().GetFloor.ToArray();
            foreach (Item item in tmpItems.ToList()) {
                if (item == null)
                    continue;

                if (!AddItemToMap(item))
                    continue;
            }

            Array.Clear(tmpItems, 0, tmpItems.Length);

            if (_room.RoomBlockingEnabled == 0) {
                foreach (RoomUser user in _room.GetRoomUserManager().GetUserList().ToList()) {
                    if (user == null)
                        continue;

                    user.SqState = GameMap[user.X, user.Y];
                    GameMap[user.X, user.Y] = 0;
                }
            }

            try {
                GameMap[Model.DoorX, Model.DoorY] = 3;
            } catch {
            }

            MarkHeightMapDirty();
        }

        private bool ConstructMapForItem(Item item, Point coord)
        {
            try {
                if (coord.X > (Model.MapSizeX - 1)) {
                    Model.AddX();
                    GenerateMaps();
                    return false;
                }

                if (coord.Y > (Model.MapSizeY - 1)) {
                    Model.AddY();
                    GenerateMaps();
                    return false;
                }

                if (Model.SqState[coord.X, coord.Y] == SquareState.Blocked) {
                    Model.OpenSquare(coord.X, coord.Y, item.GetZ);
                }

                if (_itemHeightMap[coord.X, coord.Y] <= item.TotalHeight) {
                    _itemHeightMap[coord.X, coord.Y] = item.TotalHeight - Model.SqFloorHeight[item.GetX, item.GetY];
                    EffectMap[coord.X, coord.Y] = 0;

                    switch (item.GetBaseItem().InteractionType) {
                        case InteractionType.Pool:
                            EffectMap[coord.X, coord.Y] = 1;
                            break;
                        case InteractionType.NormalSkates:
                            EffectMap[coord.X, coord.Y] = 2;
                            break;
                        case InteractionType.IceSkates:
                            EffectMap[coord.X, coord.Y] = 3;
                            break;
                        case InteractionType.LowPool:
                            EffectMap[coord.X, coord.Y] = 4;
                            break;
                        case InteractionType.HalloweenPool:
                            EffectMap[coord.X, coord.Y] = 5;
                            break;
                    }

                    //SwimHalloween
                    if (item.GetBaseItem().Walkable) // If this item is walkable and on the floor, allow users to walk here.
                    {
                        if (GameMap[coord.X, coord.Y] != 3)
                            GameMap[coord.X, coord.Y] = 1;
                    } else if (item.GetZ <= (Model.SqFloorHeight[item.GetX, item.GetY] + 0.1) && item.GetBaseItem().InteractionType == InteractionType.Gate && item.ExtraData == "1") // If this item is a gate, open, and on the floor, allow users to walk here.
                      {
                        if (GameMap[coord.X, coord.Y] != 3)
                            GameMap[coord.X, coord.Y] = 1;
                    } else if (item.GetBaseItem().IsSeat || item.GetBaseItem().InteractionType == InteractionType.Bed || item.GetBaseItem().InteractionType == InteractionType.TentSmall) {
                        GameMap[coord.X, coord.Y] = 3;
                    } else // Finally, if it's none of those, block the square.
                      {
                        if (GameMap[coord.X, coord.Y] != 3)
                            GameMap[coord.X, coord.Y] = 0;
                    }
                }

                // Set bad maps
                if (item.GetBaseItem().InteractionType == InteractionType.Bed || item.GetBaseItem().InteractionType == InteractionType.TentSmall)
                    GameMap[coord.X, coord.Y] = 3;
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
            }

            return true;
        }

        public void AddCoordinatedItem(Item item, Point coord)
        {
            if (!_coordinatedItems.TryGetValue(coord, out List<int> items)) {
                items = new List<int>();

                if (!items.Contains(item.Id))
                    items.Add(item.Id);

                if (!_coordinatedItems.ContainsKey(coord))
                    _coordinatedItems.TryAdd(coord, items);
            } else {
                if (!items.Contains(item.Id)) {
                    items.Add(item.Id);
                    _coordinatedItems[coord] = items;
                }
            }
        }

        public List<Item> GetCoordinatedItems(Point coord)
        {
            var point = new Point(coord.X, coord.Y);

            if (_coordinatedItems.ContainsKey(point)) {
                List<int> ids = _coordinatedItems[point];
                List<Item> items = GetItemsFromIds(ids);
                return items;
            }

            return new List<Item>();
        }

        public bool RemoveCoordinatedItem(Item item, Point coord)
        {
            Point point = new(coord.X, coord.Y);
            if (_coordinatedItems != null && _coordinatedItems.ContainsKey(point)) {
                _coordinatedItems[point].RemoveAll(x => x == item.Id);
                return true;
            }

            return false;
        }

        private void AddSpecialItems(Item item)
        {
            switch (item.GetBaseItem().InteractionType) {
                case InteractionType.FootballGate:
                    //IsTrans = true;
                    _room.GetSoccer().RegisterGate(item);

                    string[] splittedExtraData = item.ExtraData.Split(':');

                    if (string.IsNullOrEmpty(item.ExtraData) || splittedExtraData.Length <= 1) {
                        item.Gender = "M";
                        switch (item.Team) {
                            case Team.Yellow:
                                item.Figure = "lg-275-93.hr-115-61.hd-207-14.ch-265-93.sh-305-62";
                                break;
                            case Team.Red:
                                item.Figure = "lg-275-96.hr-115-61.hd-180-3.ch-265-96.sh-305-62";
                                break;
                            case Team.Green:
                                item.Figure = "lg-275-102.hr-115-61.hd-180-3.ch-265-102.sh-305-62";
                                break;
                            case Team.Blue:
                                item.Figure = "lg-275-108.hr-115-61.hd-180-3.ch-265-108.sh-305-62";
                                break;
                        }
                    } else {
                        item.Gender = splittedExtraData[0];
                        item.Figure = splittedExtraData[1];
                    }

                    break;

                case InteractionType.BanzaiFloor: {
                        _room.GetBanzai().AddTile(item, item.Id);
                        break;
                    }

                case InteractionType.BanzaiPyramid: {
                        _room.GetGameItemHandler().AddPyramid(item, item.Id);
                        break;
                    }

                case InteractionType.BanzaiTele: {
                        _room.GetGameItemHandler().AddTeleport(item, item.Id);
                        item.ExtraData = "";
                        break;
                    }
                case InteractionType.BanzaiPuck: {
                        _room.GetBanzai().AddPuck(item);
                        break;
                    }

                case InteractionType.Football: {
                        _room.GetSoccer().AddBall(item);
                        break;
                    }
                case InteractionType.FreezeTileBlock: {
                        _room.GetFreeze().AddFreezeBlock(item);
                        break;
                    }
                case InteractionType.FreezeTile: {
                        _room.GetFreeze().AddFreezeTile(item);
                        break;
                    }
                case InteractionType.FreezeExit: {
                        _room.GetFreeze().AddExitTile(item);
                        break;
                    }
            }
        }

        private void RemoveSpecialItem(Item item)
        {
            switch (item.GetBaseItem().InteractionType) {
                case InteractionType.FootballGate:
                    _room.GetSoccer().UnRegisterGate(item);
                    break;
                case InteractionType.BanzaiFloor:
                    _room.GetBanzai().RemoveTile(item.Id);
                    break;
                case InteractionType.BanzaiPuck:
                    _room.GetBanzai().RemovePuck(item.Id);
                    break;
                case InteractionType.BanzaiPyramid:
                    _room.GetGameItemHandler().RemovePyramid(item.Id);
                    break;
                case InteractionType.BanzaiTele:
                    _room.GetGameItemHandler().RemoveTeleport(item.Id);
                    break;
                case InteractionType.Football:
                    _room.GetSoccer().RemoveBall(item.Id);
                    break;
                case InteractionType.FreezeTile:
                    _room.GetFreeze().RemoveFreezeTile(item.Id);
                    break;
                case InteractionType.FreezeTileBlock:
                    _room.GetFreeze().RemoveFreezeBlock(item.Id);
                    break;
                case InteractionType.FreezeExit:
                    _room.GetFreeze().RemoveExitTile(item.Id);
                    break;
            }
        }

        public bool RemoveFromMap(Item item, bool handleGameItem)
        {
            if (handleGameItem)
                RemoveSpecialItem(item);

            if (_room.GotSoccer())
                _room.GetSoccer().OnGateRemove(item);

            bool isRemoved = false;
            foreach (Point coord in GetFootprintTiles(item).ToList()) {
                if (RemoveCoordinatedItem(item, coord))
                    isRemoved = true;
            }

            ConcurrentDictionary<Point, List<Item>> items = new();
            foreach (Point tile in GetFootprintTiles(item).ToList()) {
                Point point = new(tile.X, tile.Y);
                if (_coordinatedItems.ContainsKey(point)) {
                    List<int> ids = _coordinatedItems[point];
                    List<Item> itemsFromIds = GetItemsFromIds(ids);

                    if (!items.ContainsKey(tile))
                        items.TryAdd(tile, itemsFromIds);
                }

                SetDefaultValue(tile.X, tile.Y);
            }

            foreach (Point coord in items.Keys.ToList()) {
                if (!items.ContainsKey(coord))
                    continue;

                List<Item> subItems = items[coord];
                foreach (Item Item in subItems.ToList()) {
                    ConstructMapForItem(Item, coord);
                }
            }

            items.Clear();

            MarkHeightMapDirty(item);

            return isRemoved;
        }

        public bool RemoveFromMap(Item item)
        {
            return RemoveFromMap(item, true);
        }

        public bool AddItemToMap(Item item, bool handleGameItem, bool newItem = true)
        {
            if (handleGameItem) {
                AddSpecialItems(item);

                switch (item.GetBaseItem().InteractionType) {
                    case InteractionType.FootballGoalRed:
                    case InteractionType.FootballCounterRed:
                    case InteractionType.BanzaiScoreRed:
                    case InteractionType.BanzaiGateRed:
                    case InteractionType.FreezeRedCounter:
                    case InteractionType.FreezeRedGate: {
                            if (!_room.GetRoomItemHandler().GetFloor.Contains(item))
                                _room.GetGameManager().AddFurnitureToTeam(item, Team.Red);
                            break;
                        }
                    case InteractionType.FootballGoalGreen:
                    case InteractionType.FootballCounterGreen:
                    case InteractionType.BanzaiScoreGreen:
                    case InteractionType.BanzaiGateGreen:
                    case InteractionType.FreezeGreenCounter:
                    case InteractionType.FreezeGreenGate: {
                            if (!_room.GetRoomItemHandler().GetFloor.Contains(item))
                                _room.GetGameManager().AddFurnitureToTeam(item, Team.Green);
                            break;
                        }
                    case InteractionType.FootballGoalBlue:
                    case InteractionType.FootballCounterBlue:
                    case InteractionType.BanzaiScoreBlue:
                    case InteractionType.BanzaiGateBlue:
                    case InteractionType.FreezeBlueCounter:
                    case InteractionType.FreezeBlueGate: {
                            if (!_room.GetRoomItemHandler().GetFloor.Contains(item))
                                _room.GetGameManager().AddFurnitureToTeam(item, Team.Blue);
                            break;
                        }
                    case InteractionType.FootballGoalYellow:
                    case InteractionType.FootballCounterYellow:
                    case InteractionType.BanzaiScoreYellow:
                    case InteractionType.BanzaiGateYellow:
                    case InteractionType.FreezeYellowCounter:
                    case InteractionType.FreezeYellowGate: {
                            if (!_room.GetRoomItemHandler().GetFloor.Contains(item))
                                _room.GetGameManager().AddFurnitureToTeam(item, Team.Yellow);
                            break;
                        }
                    case InteractionType.FreezeExit: {
                            _room.GetFreeze().AddExitTile(item);
                            break;
                        }
                    case InteractionType.Roller: {
                            if (!_room.GetRoomItemHandler().GetRollers().Contains(item))
                                _room.GetRoomItemHandler().TryAddRoller(item.Id, item);
                            break;
                        }
                }
            }

            if (item.GetBaseItem().Type != 's')
                return true;

            MarkHeightMapDirty(item);

            foreach (Point coord in GetFootprintTiles(item).ToList()) {
                AddCoordinatedItem(item, new Point(coord.X, coord.Y));
            }

            if (item.GetX > (Model.MapSizeX - 1)) {
                Model.AddX();
                GenerateMaps();
                return false;
            }

            if (item.GetY > (Model.MapSizeY - 1)) {
                Model.AddY();
                GenerateMaps();
                return false;
            }

            bool @return = true;

            foreach (Point coord in item.GetCoords) {
                if (!ConstructMapForItem(item, coord)) {
                    @return = false;
                } else {
                    @return = true;
                }
            }

            return @return;
        }

        public bool CanWalk(int x, int y, bool @override)
        {
            if (@override) {
                return true;
            }

            if (_room.GetRoomUserManager().GetUserForSquare(x, y) != null && _room.RoomBlockingEnabled == 0)
                return false;

            return true;
        }

        public bool AddItemToMap(Item item, bool newItem = true)
        {
            return AddItemToMap(item, true, newItem);
        }

        public bool ItemCanMove(Item item, Point moveTo)
        {
            List<ThreeDCoord> points = GetAffectedTiles(item.GetBaseItem().Length, item.GetBaseItem().Width, moveTo.X, moveTo.Y, item.Rotation).Values.ToList();

            if (points == null || points.Count == 0)
                return true;

            foreach (ThreeDCoord coord in points) {
                if (coord.X >= Model.MapSizeX || coord.Y >= Model.MapSizeY)
                    return false;

                if (!SquareIsOpen(coord.X, coord.Y, false))
                    return false;
            }

            return true;
        }

        public byte GetFloorStatus(Point coord)
        {
            if (coord.X > GameMap.GetUpperBound(0) || coord.Y > GameMap.GetUpperBound(1))
                return 1;

            return GameMap[coord.X, coord.Y];
        }

        public void SetFloorStatus(int x, int y, byte status)
        {
            GameMap[x, y] = status;
        }

        public double GetHeightForSquareFromData(Point coord)
        {
            if (coord.X > Model.SqFloorHeight.GetUpperBound(0) ||
                coord.Y > Model.SqFloorHeight.GetUpperBound(1))
                return 1;
            return Model.SqFloorHeight[coord.X, coord.Y];
        }

        public bool CanRollItemHere(int x, int y)
        {
            if (!ValidTile(x, y))
                return false;

            return Model.SqState[x, y] != SquareState.Blocked;
        }

        public bool SquareIsOpen(int x, int y, bool pOverride)
        {
            if ((Model.MapSizeX - 1) < x || (Model.MapSizeY - 1) < y)
                return false;

            return CanWalk(GameMap[x, y], pOverride);
        }

        public bool GetHighestItemForSquare(Point square, out Item item)
        {
            List<Item> items = GetAllRoomItemForSquare(square.X, square.Y);
            item = null;
            double highestZ = -1;

            if (items != null && items.Any()) {
                foreach (Item uItem in items.ToList()) {
                    if (uItem == null)
                        continue;

                    if (uItem.TotalHeight > highestZ) {
                        highestZ = uItem.TotalHeight;
                        item = uItem;
                    }
                }
            } else
                return false;

            return true;
        }

        public double GetHeightForSquare(Point coord)
        {
            if (GetHighestItemForSquare(coord, out Item rItem))
                if (rItem != null)
                    return rItem.TotalHeight;

            return 0.0;
        }

        public Point GetChaseMovement(Item item)
        {
            int distance = 99;
            Point coord = new(0, 0);
            int iX = item.GetX;
            int iY = item.GetY;
            bool x = false;

            foreach (RoomUser user in _room.GetRoomUserManager().GetRoomUsers()) {
                if (user.X == item.GetX || item.GetY == user.Y) {
                    if (user.X == item.GetX) {
                        int difference = Math.Abs(user.Y - item.GetY);
                        if (difference < distance) {
                            distance = difference;
                            coord = user.Coordinate;
                            x = false;
                        }
                    } else if (user.Y == item.GetY) {
                        int difference = Math.Abs(user.X - item.GetX);
                        if (difference < distance) {
                            distance = difference;
                            coord = user.Coordinate;
                            x = true;
                        }
                    }
                }
            }

            if (distance > 5)
                return item.GetSides().OrderBy(x => Guid.NewGuid()).FirstOrDefault();

            if (x && distance < 99) {
                if (iX > coord.X) {
                    iX--;
                    return new Point(iX, iY);
                }

                iX++;
                return new Point(iX, iY);
            }

            if (!x && distance < 99) {
                if (iY > coord.Y) {
                    iY--;
                    return new Point(iX, iY);
                }

                iY++;
                return new Point(iX, iY);
            }

            return item.Coordinate;
        }

        #region 3D height map

        /// <summary>
        /// Current 3D snapshot of the room, rebuilt on demand after furniture changes.
        /// </summary>
        public RoomHeightMap GetHeightMap()
        {
            if (_heightMap == null || _heightMapDirty || _heightMapDirtyTileCount > 0)
                RebuildHeightMap();

            return _heightMap;
        }

        /// <summary>Flags the whole 3D snapshot as stale. Only for changes that can move every
        /// tile — a model resize or a full map regeneration.</summary>
        public void MarkHeightMapDirty()
        {
            _heightMapDirty = true;
        }

        /// <summary>
        /// Flags just the tiles one item covers. Rollers, gates and wired move furniture on nearly
        /// every cycle, and rebuilding a whole room for each of them was enough to show up as a
        /// hitch in the movement tick.
        /// </summary>
        public void MarkHeightMapDirty(Item item)
        {
            if (item?.GetBaseItem() == null || item.GetBaseItem().Type != 's') {
                return;
            }

            lock (_heightMapLock) {
                if (_heightMapDirty)
                    return; // a full rebuild is already queued, no point listing tiles

                foreach (Point coord in GetFootprintTiles(item)) {
                    if (ValidTile(coord.X, coord.Y))
                        _heightMapDirtyTiles.Add((coord.Y * Model.MapSizeX) + coord.X);
                }

                _heightMapDirtyTileCount = _heightMapDirtyTiles.Count;
            }
        }

        public void OnItemHeightChanged(Item item)
        {
            if (item?.GetBaseItem() == null || item.GetBaseItem().Type != 's')
                return;

            MarkHeightMapDirty(item);
            UpdateOccupantHeights(item);
        }

        public void UpdateOccupantHeights(Item item)
        {
            if (item?.GetBaseItem() == null || item.GetBaseItem().Type != 's')
                return;

            RoomUserManager userManager = _room.GetRoomUserManager();
            if (userManager == null)
                return;

            foreach (Point coord in GetFootprintTiles(item)) {
                if (!ValidTile(coord.X, coord.Y))
                    continue;

                foreach (RoomUser user in GetRoomUsers(coord)) {
                    if (user == null)
                        continue;

                    userManager.UpdateUserStatus(user, false);
                }
            }
        }

        public static void GetFootprintSpan(int width, int length, int rotation, out int spanX, out int spanY)
        {
            width = Math.Max(1, width);
            length = Math.Max(1, length);

            rotation = ((rotation % 8) + 8) % 8;
            bool swapped = rotation == 2 || rotation == 3 || rotation == 6 || rotation == 7;

            spanX = swapped ? length : width;
            spanY = swapped ? width : length;
        }

        private static void GetFootprintSpan(Item item, out int spanX, out int spanY)
        {
            ItemData data = item.GetBaseItem();

            GetFootprintSpan(data.Width, data.Length, item.Rotation, out spanX, out spanY);
        }

        private static IEnumerable<Point> GetFootprintTiles(Item item)
        {
            GetFootprintSpan(item, out int spanX, out int spanY);

            for (int dx = 0; dx < spanX; dx++) {
                for (int dy = 0; dy < spanY; dy++)
                    yield return new Point(item.GetX + dx, item.GetY + dy);
            }
        }

        public void RebuildHeightMap()
        {
            lock (_heightMapLock) {
                int width = Model.MapSizeX;
                int height = Model.MapSizeY;

                // Anything short of a full rebuild edits a copy of the live snapshot, because a
                // path search may still be reading the old one on another thread.
                bool incremental = !_heightMapDirty && _heightMap != null &&
                                   _heightMap.Width == width && _heightMap.Height == height;

                if (incremental) {
                    var patch = new RoomHeightMap.Builder(_heightMap.Clone());

                    foreach (int tile in _heightMapDirtyTiles) {
                        int x = tile % width;
                        int y = tile / width;

                        if (!ValidTile(x, y))
                            continue;

                        BuildHeightMapTile(patch, x, y, GetAllRoomItemForSquare(x, y));
                    }

                    _heightMapDirtyTiles.Clear();
                    _heightMapDirtyTileCount = 0;
                    _heightMap = patch.Build();
                    return;
                }

                var builder = new RoomHeightMap.Builder(width, height);
                List<Item>[] tiles = new List<Item>[width * height];

                foreach (Item item in _room.GetRoomItemHandler().GetFloor.ToList()) {
                    if (item?.GetBaseItem() == null || item.GetBaseItem().Type != 's')
                        continue;

                    foreach (Point coord in GetFootprintTiles(item)) {
                        if (!ValidTile(coord.X, coord.Y))
                            continue;

                        int index = (coord.Y * width) + coord.X;
                        List<Item> bucket = tiles[index] ??= new List<Item>();

                        if (!bucket.Contains(item))
                            bucket.Add(item);
                    }
                }

                for (int y = 0; y < height; y++) {
                    for (int x = 0; x < width; x++)
                        BuildHeightMapTile(builder, x, y, tiles[(y * width) + x]);
                }

                _heightMapDirtyTiles.Clear();
                _heightMapDirtyTileCount = 0;
                _heightMap = builder.Build();
                _heightMapDirty = false;
            }
        }

        private void BuildHeightMapTile(RoomHeightMap.Builder builder, int x, int y, List<Item> itemsOnTile)
        {
            bool isDoor = x == Model.DoorX && y == Model.DoorY;
            double floorHeight = Model.SqFloorHeight[x, y];

            if (isDoor)
                builder.AddSurface(0, floorHeight, RoomHeightMap.StateLastStep, 0);
            else if (Model.SqState[x, y] == SquareState.Open)
                builder.AddSurface(0, floorHeight, RoomHeightMap.StateOpen, 0);
            else if (Model.SqState[x, y] == SquareState.Seat)
                builder.AddSurface(0, floorHeight, RoomHeightMap.StateSeat, 0);

            if (itemsOnTile != null) {
                foreach (Item item in itemsOnTile)
                    AddItemToHeightMap(builder, item, floorHeight);
            }

            builder.CommitTile(x, y, isDoor);
        }

        private static void AddItemToHeightMap(RoomHeightMap.Builder builder, Item item, double floorHeight)
        {
            ItemData data = item.GetBaseItem();

            builder.AddFurniture(item.Id, ClassifyFurniture(item, data, floorHeight), item.GetZ, item.TotalHeight, item.EffectiveHeight, floorHeight, item.GroupId);
        }

        private static FurnitureFootprint ClassifyFurniture(Item item, ItemData data, double floorHeight)
        {
            InteractionType type = data.InteractionType;

            if (type == InteractionType.GuildGate)
                return FurnitureFootprint.GuildGate;

            if (data.IsSeat || type == InteractionType.Bed || type == InteractionType.TentSmall)
                return FurnitureFootprint.Seat;

            if (data.WalkableState == 2)
                return FurnitureFootprint.WalkableStopOnly;

            if (data.WalkableState == 1)
                return FurnitureFootprint.Walkable;

            // An open gate lying on the floor is walked straight through.
            if (type == InteractionType.Gate && item.ExtraData == "1" && item.GetZ <= floorHeight + 0.1)
                return FurnitureFootprint.Walkable;

            return FurnitureFootprint.Solid;
        }

        public PathContext CreatePathContext(RoomUser user, byte[] occupancy)
        {
            RoomHeightMap map = GetHeightMap();

            return new PathContext
            {
                HeightMap = map,
                DiagonalEnabled = DiagonalEnabled,
                AllowOverride = user != null && user.AllowOverride,
                IgnoreHeightLimits = user != null && user.RidingHorse,
                UsersBlockTiles = _room.RoomBlockingEnabled == 0,
                Occupancy = occupancy,
                IgnoreTile = user != null && ValidTile(user.X, user.Y) ? (user.Y * map.Width) + user.X : -1,
                OpenGateGroups = ResolveOpenGateGroups(user, map)
            };
        }

        private static HashSet<int> ResolveOpenGateGroups(RoomUser user, RoomHeightMap map)
        {
            if (map.GateGroupIds.Length == 0 || user == null || user.IsBot)
                return null;

            int habboId = user.GetClient()?.GetHabbo()?.Id ?? 0;
            if (habboId == 0)
                return null;

            HashSet<int> open = null;

            foreach (int groupId in map.GateGroupIds) {
                if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out Group group))
                    continue;

                if (group.IsMember(habboId))
                    (open ??= new HashSet<int>()).Add(groupId);
            }

            return open;
        }

        #endregion

        public bool IsValidStep2(RoomUser user, Vector2D from, Vector2D to, bool endOfPath, bool @override)
        {
            return IsValidStep2(user, from, to, SqAbsoluteHeight(to.X, to.Y), endOfPath, @override);
        }

        public bool IsValidStep2(RoomUser user, Vector2D from, Vector2D to, double toZ, bool endOfPath, bool @override)
        {
            if (user == null)
                return false;

            if (!ValidTile(to.X, to.Y))
                return false;

            if (@override)
                return true;

            /*
             * 0 = blocked
             * 1 = open
             * 2 = last step
             * 3 = door
             * */

            List<Item> items = _room.GetGameMap().GetAllRoomItemForSquare(to.X, to.Y);
            if (items.Count > 0) {
                bool hasGroupGate = items.Any(x => x.GetBaseItem().InteractionType == InteractionType.GuildGate);
                if (hasGroupGate) {
                    Item I = items.FirstOrDefault(x => x.GetBaseItem().InteractionType == InteractionType.GuildGate);
                    if (I != null) {
                        if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(I.GroupId, out Group group))
                            return false;

                        if (user.GetClient() == null || user.GetClient().GetHabbo() == null)
                            return false;

                        if (group.IsMember(user.GetClient().GetHabbo().Id)) {
                            I.InteractingUser = user.GetClient().GetHabbo().Id;
                            I.ExtraData = "1";
                            I.UpdateState(false, true);

                            I.RequestUpdate(4, true);

                            return true;
                        }

                        if (user.Path.Count > 0)
                            user.Path.Clear();
                        user.PathRecalcNeeded = false;
                        return false;
                    }
                }
            }

            RoomHeightMap map = GetHeightMap();
            int level = map.FindLevel(to.X, to.Y, toZ);

            if (level < 0) {
                if (user.Path.Count > 0)
                    user.Path.Clear();
                user.PathRecalcNeeded = true;
                return false;
            }

            byte state = map.StateAt(map.TileIndex(to.X, to.Y), level);

            // Legacy behaviour, recalc if path blocked.
            if (state == RoomHeightMap.StateBlocked ||
                ((state == RoomHeightMap.StateSeat || state == RoomHeightMap.StateLastStep) && !endOfPath)) {
                if (user.Path.Count > 0)
                    user.Path.Clear();
                user.PathRecalcNeeded = true;
            }

            if (!user.RidingHorse) {
                double fromZ = map.SurfaceNear(from.X, from.Y, user.Z);
                if (!RoomHeightMap.CanTransition(fromZ, toZ))
                    return false;
            }

            //Check this last, because ya.
            //When room blocking is enabled (RoomBlockingEnabled != 0) users may share the exact same tile.
            RoomUser userX = _room.GetRoomUserManager().GetUserForSquare(to.X, to.Y);
            if (userX != null && _room.RoomBlockingEnabled == 0) {
                if (!userX.IsWalking && endOfPath)
                    return false;
            }

            return true;
        }

        public bool IsValidStep(Vector2D from, Vector2D to, bool endOfPath, bool overriding, bool roller = false)
        {
            if (!ValidTile(to.X, to.Y))
                return false;

            if (overriding)
                return true;

            /*
             * 0 = blocked
             * 1 = open
             * 2 = last step
             * 3 = door
             * */

            if (_room.RoomBlockingEnabled == 0 && SquareHasUsers(to.X, to.Y)) {
                if (!endOfPath)
                    return false;

                RoomUser occupant = _room.GetRoomUserManager().GetUserForSquare(to.X, to.Y);
                if (occupant != null && !occupant.IsWalking)
                    return false;
            }

            // This runs for every neighbour probe of every pathfind, so the (allocating) item
            // lookup is deferred until the tile-state check actually fails — a guild gate on a
            // blocked square is the only thing that can rescue the step.
            byte state = GameMap[to.X, to.Y];
            if (state == 3 && !endOfPath || state == 0 || state == 2 && !endOfPath) {
                List<Item> items = GetAllRoomItemForSquare(to.X, to.Y);
                return items.Any(x => x != null && x.GetBaseItem().InteractionType == InteractionType.GuildGate);
            }

            if (!roller) {
                double heightDiff = SqAbsoluteHeight(to.X, to.Y) - SqAbsoluteHeight(from.X, from.Y);
                if (heightDiff > 1.5)
                    return false;
            }

            return true;
        }

        public static bool CanWalk(byte state, bool overriding)
        {
            if (!overriding) {
                if (state == 3)
                    return true;
                if (state == 1)
                    return true;

                return false;
            }

            return true;
        }

        public bool ItemCanBePlaced(int x, int y)
        {
            if (Model.MapSizeX - 1 < x || Model.MapSizeY - 1 < y ||
                (x == Model.DoorX && y == Model.DoorY))
                return false;

            return GameMap[x, y] == 1;
        }

        public double SqAbsoluteHeight(int x, int y)
        {
            Point points = new(x, y);

            if (_coordinatedItems.TryGetValue(points, out List<int> ids)) {
                List<Item> items = GetItemsFromIds(ids);

                return SqAbsoluteHeight(x, y, items);
            }

            return Model.SqFloorHeight[x, y];

            #region Old

            /*
            if (mCoordinatedItems.ContainsKey(Points)) {
                List<Item> Items = new List<Item>();
                foreach (Item Item in mCoordinatedItems[Points].ToArray())
                {
                    if (!Items.Contains(Item))
                        Items.Add(Item);
                }
                return SqAbsoluteHeight(X, Y, Items);
            }*/

            #endregion
        }

        public double SqAbsoluteHeight(int x, int y, List<Item> itemsOnSquare)
        {
            try {
                bool deduct = false;
                double highestStack = 0;
                double deductible = 0.0;

                if (itemsOnSquare != null && itemsOnSquare.Count > 0) {
                    foreach (Item item in itemsOnSquare.ToList()) {
                        if (item == null)
                            continue;

                        if (item.TotalHeight > highestStack) {
                            if (item.GetBaseItem().IsSeat || item.GetBaseItem().InteractionType == InteractionType.Bed || item.GetBaseItem().InteractionType == InteractionType.TentSmall) {
                                deduct = true;
                                deductible = item.EffectiveHeight;
                            } else
                                deduct = false;

                            highestStack = item.TotalHeight;
                        }
                    }
                }

                double floorHeight = Model.SqFloorHeight[x, y];
                double stackHeight = highestStack - Model.SqFloorHeight[x, y];

                if (deduct)
                    stackHeight -= deductible;

                if (stackHeight < 0)
                    stackHeight = 0;

                return floorHeight + stackHeight;
            } catch (Exception e) {
                ExceptionLogger.LogException(e);
                return 0;
            }
        }

        public bool ValidTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Model.MapSizeX || y >= Model.MapSizeY)
                return false;

            return true;
        }

        public static Dictionary<int, ThreeDCoord> GetAffectedTiles(int length, int width, int posX, int posY, int rotation)
        {
            var pointList = new Dictionary<int, ThreeDCoord>();

            GetFootprintSpan(width, length, rotation, out int spanX, out int spanY);

            int index = 0;

            for (int dx = 0; dx < spanX; dx++) {
                for (int dy = 0; dy < spanY; dy++) {
                    if (dx == 0 && dy == 0)
                        continue;

                    pointList.Add(index++, new ThreeDCoord(posX + dx, posY + dy, Math.Max(dx, dy)));
                }
            }

            return pointList;
        }

        public List<Item> GetItemsFromIds(List<int> input)
        {
            if (input == null || input.Count == 0)
                return new List<Item>();

            List<Item> items = new(input.Count);

            lock (input) {
                foreach (int id in input) {
                    Item item = _room.GetRoomItemHandler().GetItem(id);
                    if (item != null && !items.Contains(item))
                        items.Add(item);
                }
            }

            return items;
        }

        public List<Item> GetRoomItemForSquare(int pX, int pY, double minZ)
        {
            var itemsToReturn = new List<Item>();

            var coord = new Point(pX, pY);
            if (_coordinatedItems.ContainsKey(coord)) {
                var itemsFromSquare = GetItemsFromIds(_coordinatedItems[coord]);

                foreach (Item item in itemsFromSquare)
                    if (item.GetZ > minZ)
                        if (item.GetX == pX && item.GetY == pY)
                            itemsToReturn.Add(item);
            }

            return itemsToReturn;
        }

        public List<Item> GetRoomItemForSquare(int pX, int pY)
        {
            var coord = new Point(pX, pY);
            //List<RoomItem> itemsFromSquare = new List<RoomItem>();
            var itemsToReturn = new List<Item>();

            if (_coordinatedItems.ContainsKey(coord)) {
                var itemsFromSquare = GetItemsFromIds(_coordinatedItems[coord]);

                foreach (Item item in itemsFromSquare) {
                    if (item.Coordinate.X == coord.X && item.Coordinate.Y == coord.Y)
                        itemsToReturn.Add(item);
                }
            }

            return itemsToReturn;
        }

        public List<Item> GetAllRoomItemForSquare(int x, int y)
        {
            Point coord = new(x, y);

            List<Item> items;

            if (_coordinatedItems.TryGetValue(coord, out List<int> ids))
                items = GetItemsFromIds(ids);
            else
                items = new List<Item>();

            return items;
        }

        public bool TryGetBedRestTile(int x, int y, out Point restTile)
        {
            restTile = new Point(x, y);

            if (!ValidTile(x, y))
                return false;

            Item bed = null;

            foreach (Item item in GetAllRoomItemForSquare(x, y)) {
                ItemData data = item?.GetBaseItem();

                if (data == null || (data.InteractionType != InteractionType.Bed && data.InteractionType != InteractionType.TentSmall))
                    continue;

                if (bed == null || item.TotalHeight > bed.TotalHeight)
                    bed = item;
            }

            if (bed == null)
                return false;

            GetFootprintSpan(bed, out int spanX, out int spanY);

            int rotation = ((bed.Rotation % 8) + 8) % 8;
            bool headAtFarEnd = rotation >= 4;

            if (spanX >= spanY) {
                int headX = headAtFarEnd ? bed.GetX + spanX - 1 : bed.GetX;
                restTile = new Point(headX, Math.Clamp(y, bed.GetY, bed.GetY + spanY - 1));
            } else {
                int headY = headAtFarEnd ? bed.GetY + spanY - 1 : bed.GetY;
                restTile = new Point(Math.Clamp(x, bed.GetX, bed.GetX + spanX - 1), headY);
            }

            return restTile.X != x || restTile.Y != y;
        }

        public bool SquareHasUsers(int x, int y)
        {
            return MapGotUser(new Point(x, y));
        }

        public bool SquareHasUsers(int x, int y, RoomUser excluding)
        {
            if (excluding == null)
                return SquareHasUsers(x, y);

            if (!_userMap.TryGetValue(new Point(x, y), out List<RoomUser> users))
                return false;

            lock (users) {
                foreach (RoomUser occupant in users) {
                    if (occupant != null && occupant.VirtualId != excluding.VirtualId)
                        return true;
                }
            }

            return false;
        }

        public static bool TilesTouching(int x1, int y1, int x2, int y2)
        {
            if (!(Math.Abs(x1 - x2) > 1 || Math.Abs(y1 - y2) > 1))
                return true;

            return x1 == x2 && y1 == y2;
        }

        public static int TileDistance(int x1, int y1, int x2, int y2)
        {
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        public DynamicRoomModel Model { get; private set; }

        public RoomModel StaticModel { get; private set; }

        public byte[,] EffectMap { get; private set; }

        public byte[,] GameMap { get; private set; }

        public void Dispose()
        {
            _userMap.Clear();
            Model.Destroy();
            _coordinatedItems.Clear();

            Array.Clear(GameMap, 0, GameMap.Length);
            Array.Clear(EffectMap, 0, EffectMap.Length);
            Array.Clear(_itemHeightMap, 0, _itemHeightMap.Length);

            _userMap = null;
            GameMap = null;
            EffectMap = null;
            _itemHeightMap = null;
            _coordinatedItems = null;

            Model = null;
            _room = null;
            StaticModel = null;
        }
    }
}