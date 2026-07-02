using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Rooms.Freeze;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Wired;
using Plus.HabboHotel.Rooms.Games.Teams;

namespace Plus.HabboHotel.Rooms.Games.Freeze
{
    public class Freeze
    {
        private Room _room;
        private Random _random;
        private readonly ConcurrentDictionary<int, Item> _freezeBlocks;
        private readonly ConcurrentDictionary<int, Item> _freezeTiles;

        public Freeze(Room room)
        {
            _room = room;
            GameIsStarted = false;
            ExitTeleports = new ConcurrentDictionary<int, Item>();
            _random = new Random();
            _freezeTiles = new ConcurrentDictionary<int, Item>();
            _freezeBlocks = new ConcurrentDictionary<int, Item>();
        }

        public bool GameIsStarted { get; private set; }

        public ConcurrentDictionary<int, Item> ExitTeleports { get; }

        public void AddExitTile(Item item)
        {
            if (!ExitTeleports.ContainsKey(item.Id))
                ExitTeleports.TryAdd(item.Id, item);
        }

        public void RemoveExitTile(int id)
        {
            if (ExitTeleports.ContainsKey(id))
                ExitTeleports.TryRemove(id, out Item temp);
        }

        public Item GetRandomExitTile()
        {
            return ExitTeleports.Values.ToList()[PlusEnvironment.GetRandomNumber(0, ExitTeleports.Count - 1)];
        }

        public void StartGame()
        {
            GameIsStarted = true;
            CountTeamPoints();
            ResetGame();

            if (ExitTeleports.Count > 0) {
                foreach (Item exitTile in ExitTeleports.Values.ToList())
                {
                    if (exitTile.ExtraData == "0" || string.IsNullOrEmpty(exitTile.ExtraData))
                        exitTile.ExtraData = "1";

                    exitTile.UpdateState();
                }
            }

            _room.GetGameManager().LockGates();
        }

        public void StopGame(bool userTriggered = false)
        {
            GameIsStarted = false;
            _room.GetGameManager().UnlockGates();
            _room.GetGameManager().StopGame();

            ResetGame();

            if (ExitTeleports.Count > 0) {
                foreach (Item exitTile in ExitTeleports.Values.ToList())
                {
                    if (exitTile.ExtraData == "1" || string.IsNullOrEmpty(exitTile.ExtraData))
                        exitTile.ExtraData = "0";

                    exitTile.UpdateState();
                }
            }

            Team winners = _room.GetGameManager().GetWinningTeam();
            foreach (RoomUser user in _room.GetRoomUserManager().GetUserList().ToList())
            {
                user.FreezeLives = 0;
                if (user.Team == winners) {
                    user.UnIdle();
                    user.DanceId = 0;
                    _room.SendPacket(new ActionComposer(user.VirtualId, 1));
                }

                if (ExitTeleports.Count > 0) {
                    Item tile = _freezeTiles.Values.FirstOrDefault(x => x.GetX == user.X && x.GetY == user.Y);
                    if (tile != null) {
                        Item exitTile = GetRandomExitTile();

                        if (exitTile != null) {
                            _room.GetGameMap().UpdateUserMovement(user.Coordinate, exitTile.Coordinate, user);
                            user.SetPos(exitTile.GetX, exitTile.GetY, exitTile.GetZ);
                            user.UpdateNeeded = true;

                            if (user.IsAsleep)
                                user.UnIdle();
                        }
                    }
                }
            }

            if (!userTriggered)
                _room.GetWired().TriggerEvent(WiredBoxType.TriggerGameEnds, null);
        }

        public void CycleUser(RoomUser user)
        {
            if (user.Freezed) {
                user.FreezeCounter++;
                if (user.FreezeCounter > 10) {
                    user.Freezed = false;
                    user.FreezeCounter = 0;
                    ActivateShield(user);
                }
            }

            if (user.ShieldActive) {
                user.ShieldCounter++;
                if (user.ShieldCounter > 10) {
                    user.ShieldActive = false;
                    user.ShieldCounter = 10;
                    user.ApplyEffect(Convert.ToInt32(user.Team) + 39);
                }
            }
        }

        public void ResetGame()
        {
            foreach (Item item in _freezeTiles.Values.ToList())
            {
                if (!string.IsNullOrEmpty(item.ExtraData)) {
                    item.InteractionCountHelper = 0;
                    item.ExtraData = "";
                    item.UpdateState(false, true);
                    _room.GetGameMap().AddItemToMap(item, false);
                }
            }

            foreach (Item item in _freezeBlocks.Values)
            {
                if (!string.IsNullOrEmpty(item.ExtraData)) {
                    item.ExtraData = "";
                    item.UpdateState(false, true);
                    _room.GetGameMap().AddItemToMap(item, false);
                }
            }
        }

        public void OnUserWalk(RoomUser user)
        {
            if (!GameIsStarted || user.Team == Team.None)
                return;

            foreach (Item item in _freezeTiles.Values.ToList())
            {
                if (user.GoalX == item.GetX && user.GoalY == item.GetY && user.FreezeInteracting) {
                    if (item.InteractionCountHelper == 0) {
                        item.InteractionCountHelper = 1;
                        item.ExtraData = "1000";
                        item.UpdateState();
                        item.InteractingUser = user.UserId;
                        item.FreezePowerUp = user.BanzaiPowerUp;
                        item.RequestUpdate(4, true);

                        switch (user.BanzaiPowerUp)
                        {
                            case FreezePowerUp.GreenArrow:
                            case FreezePowerUp.OrangeSnowball:
                            {
                                user.BanzaiPowerUp = FreezePowerUp.None;
                                break;
                            }
                        }

                        break;
                    }
                }
            }

            foreach (Item item in _freezeBlocks.Values.ToList())
            {
                if (user.GoalX == item.GetX && user.GoalY == item.GetY) {
                    if (item.FreezePowerUp != FreezePowerUp.None) {
                        PickUpPowerUp(item, user);
                    }
                }
            }
        }

        private void CountTeamPoints()
        {
            _room.GetGameManager().Reset();

            foreach (RoomUser user in _room.GetRoomUserManager().GetUserList().ToList())
            {
                if (user.IsBot || user.Team == Team.None || user.GetClient() == null)
                    continue;

                user.BanzaiPowerUp = FreezePowerUp.None;
                user.FreezeLives = 3;
                user.ShieldActive = false;
                user.ShieldCounter = 11;

                _room.GetGameManager().AddPointToTeam(user.Team, 30);
                user.GetClient().SendPacket(new UpdateFreezeLivesComposer(user.InternalRoomId, user.FreezeLives));
            }
        }

        public void OnFreezeTiles(Item item, FreezePowerUp powerUp)
        {
            List<Item> items;

            switch (powerUp)
            {
                case FreezePowerUp.BlueArrow:
                {
                    items = GetVerticalItems(item.GetX, item.GetY, 5);
                    break;
                }

                case FreezePowerUp.GreenArrow:
                {
                    items = GetDiagonalItems(item.GetX, item.GetY, 5);
                    break;
                }

                case FreezePowerUp.OrangeSnowball:
                {
                    items = GetVerticalItems(item.GetX, item.GetY, 5);
                    items.AddRange(GetDiagonalItems(item.GetX, item.GetY, 5));
                    break;
                }

                default:
                {
                    items = GetVerticalItems(item.GetX, item.GetY, 3);
                    break;
                }
            }

            HandleBanzaiFreezeItems(items);
        }

        private static void ActivateShield(RoomUser user)
        {
            user.ApplyEffect(Convert.ToInt32(user.Team + 48));
            user.ShieldActive = true;
            user.ShieldCounter = 0;
        }

        private void HandleBanzaiFreezeItems(List<Item> items)
        {
            foreach (Item item in items.ToList())
            {
                switch (item.GetBaseItem().InteractionType)
                {
                    case InteractionType.FreezeTile:
                    {
                        item.ExtraData = "11000";
                        item.UpdateState(false, true);
                        continue;
                    }

                    case InteractionType.FreezeTileBlock:
                    {
                        SetRandomPowerUp(item);
                        item.UpdateState(false, true);
                        continue;
                    }
                    default:
                    {
                        continue;
                    }
                }
            }
        }

        private void SetRandomPowerUp(Item item)
        {
            if (!string.IsNullOrEmpty(item.ExtraData))
                return;

            int next = _random.Next(1, 14);

            switch (next)
            {
                case 2:
                {
                    item.ExtraData = "2000";
                    item.FreezePowerUp = FreezePowerUp.BlueArrow;
                    break;
                }
                case 3:
                {
                    item.ExtraData = "3000";
                    item.FreezePowerUp = FreezePowerUp.Snowballs;
                    break;
                }
                case 4:
                {
                    item.ExtraData = "4000";
                    item.FreezePowerUp = FreezePowerUp.GreenArrow;
                    break;
                }
                case 5:
                {
                    item.ExtraData = "5000";
                    item.FreezePowerUp = FreezePowerUp.OrangeSnowball;
                    break;
                }
                case 6:
                {
                    item.ExtraData = "6000";
                    item.FreezePowerUp = FreezePowerUp.Heart;
                    break;
                }
                case 7:
                {
                    item.ExtraData = "7000";
                    item.FreezePowerUp = FreezePowerUp.Shield;
                    break;
                }
                default:
                {
                    item.ExtraData = "1000";
                    item.FreezePowerUp = FreezePowerUp.None;
                    break;
                }
            }

            _room.GetGameMap().RemoveFromMap(item, false);
            item.UpdateState(false, true);
        }

        private void PickUpPowerUp(Item item, RoomUser user)
        {
            switch (item.FreezePowerUp)
            {
                case FreezePowerUp.Heart:
                {
                    if (user.FreezeLives < 5) {
                        user.FreezeLives++;
                        _room.GetGameManager().AddPointToTeam(user.Team, 10);
                    }

                    user.GetClient().SendPacket(new UpdateFreezeLivesComposer(user.InternalRoomId, user.FreezeLives));
                    break;
                }
                case FreezePowerUp.Shield:
                {
                    ActivateShield(user);
                    break;
                }
                case FreezePowerUp.BlueArrow:
                case FreezePowerUp.GreenArrow:
                case FreezePowerUp.OrangeSnowball:
                {
                    user.BanzaiPowerUp = item.FreezePowerUp;
                    break;
                }
            }

            item.FreezePowerUp = FreezePowerUp.None;
            item.ExtraData = "1" + item.ExtraData;
            item.UpdateState(false, true);
        }

        public void AddFreezeTile(Item item)
        {
            if (!_freezeTiles.ContainsKey(item.Id))
                _freezeTiles.TryAdd(item.Id, item);
        }

        public void RemoveFreezeTile(int itemId)
        {
            if (_freezeTiles.ContainsKey(itemId))
                _freezeTiles.TryRemove(itemId, out Item item);
        }

        public void AddFreezeBlock(Item item)
        {
            if (!_freezeBlocks.ContainsKey(item.Id))
                _freezeBlocks.TryAdd(item.Id, item);
        }

        public void RemoveFreezeBlock(int itemId)
        {
            _freezeBlocks.TryRemove(itemId, out Item item);
        }

        private void HandleUserFreeze(Point point)
        {
            RoomUser user = _room?.GetGameMap().GetRoomUsers(point).FirstOrDefault();
            if (user != null) {
                if (user.IsWalking && user.SetX != point.X && user.SetY != point.Y)
                    return;

                FreezeUser(user);
            }
        }

        private void FreezeUser(RoomUser user)
        {
            if (user.IsBot || user.ShieldActive || user.Team == Team.None || user.Freezed)
                return;

            user.Freezed = true;
            user.FreezeCounter = 0;

            user.FreezeLives--;
            if (user.FreezeLives <= 0) {
                user.GetClient().SendPacket(new UpdateFreezeLivesComposer(user.InternalRoomId, user.FreezeLives));

                user.ApplyEffect(-1);
                _room.GetGameManager().AddPointToTeam(user.Team, -10);
                TeamManager t = _room.GetTeamManagerForFreeze();
                t.OnUserLeave(user);
                user.Team = Team.None;
                if (ExitTeleports.Count > 0)
                    _room.GetGameMap().TeleportToItem(user, GetRandomExitTile());

                user.Freezed = false;
                user.SetStep = false;
                user.IsWalking = false;
                user.UpdateNeeded = true;

                if (t.BlueTeam.Count <= 0 && t.RedTeam.Count <= 0 && t.GreenTeam.Count <= 0 && t.YellowTeam.Count > 0)
                    StopGame(); // yellow team win
                else if (t.BlueTeam.Count > 0 && t.RedTeam.Count <= 0 && t.GreenTeam.Count <= 0 &&
                         t.YellowTeam.Count <= 0)
                    StopGame(); // blue team win
                else if (t.BlueTeam.Count <= 0 && t.RedTeam.Count > 0 && t.GreenTeam.Count <= 0 &&
                         t.YellowTeam.Count <= 0)
                    StopGame(); // red team win
                else if (t.BlueTeam.Count <= 0 && t.RedTeam.Count <= 0 && t.GreenTeam.Count > 0 &&
                         t.YellowTeam.Count <= 0)
                    StopGame(); // green team win
                return;
            }

            _room.GetGameManager().AddPointToTeam(user.Team, -10);
            user.ApplyEffect(12);

            user.GetClient().SendPacket(new UpdateFreezeLivesComposer(user.InternalRoomId, user.FreezeLives));
        }

        private List<Item> GetVerticalItems(int x, int y, int length)
        {
            var totalItems = new List<Item>();

            for (int i = 0; i < length; i++)
            {
                var point = new Point(x + i, y);

                List<Item> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (int i = 1; i < length; i++)
            {
                var point = new Point(x, y + i);

                List<Item> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (int i = 1; i < length; i++)
            {
                var point = new Point(x - i, y);
                List<Item> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (int i = 1; i < length; i++)
            {
                var point = new Point(x, y - i);
                List<Item> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            return totalItems;
        }

        private List<Item> GetDiagonalItems(int x, int y, int length)
        {
            var totalItems = new List<Item>();

            for (int i = 0; i < length; i++)
            {
                var point = new Point(x + i, y + i);

                List<Item> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (int i = 0; i < length; i++)
            {
                var point = new Point(x - i, y - i);
                List<Item> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (int i = 0; i < length; i++)
            {
                var point = new Point(x - i, y + i);
                List<Item> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (int i = 0; i < length; i++)
            {
                var point = new Point(x + i, y - i);
                List<Item> items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            return totalItems;
        }

        private List<Item> GetItemsForSquare(Point point)
        {
            return _room.GetGameMap().GetCoordinatedItems(point);
        }

        private static bool SquareGotFreezeTile(List<Item> items)
        {
            foreach (Item item in items)
            {
                if (item.GetBaseItem().InteractionType == InteractionType.FreezeTile)
                    return true;
            }

            return false;
        }

        private static bool SquareGotFreezeBlock(List<Item> items)
        {
            foreach (Item item in items)
            {
                if (item.GetBaseItem().InteractionType == InteractionType.FreezeTileBlock)
                    return true;
            }

            return false;
        }

        public void Dispose()
        {
            _room = null;
            _random = null;
            ExitTeleports.Clear();
            _freezeTiles.Clear();
            _freezeBlocks.Clear();
        }
    }
}