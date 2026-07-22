using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using System.Drawing;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Utilities;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class MoveAndRotateBox : IWiredItem, IWiredCycle
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectMoveAndRotate;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }

        public int Delay
        {
            get => _delay;
            set
            {
                _delay = value;
                TickCount = value + 1;
            }
        }

        public int TickCount { get; set; }
        public string ItemsData { get; set; }
        private bool _requested;
        private int _delay;
        private long _next;

        public MoveAndRotateBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();
            TickCount = Delay;
            _requested = false;
        }

        public void HandleSave(ClientPacket packet)
        {
            if (SetItems.Count > 0)
                SetItems.Clear();

            int unknown = packet.PopInt();
            int movement = packet.PopInt();
            int rotation = packet.PopInt();

            string unknown1 = packet.PopString();

            int furniCount = packet.PopInt();
            for (int i = 0; i < furniCount; i++) {
                Item selectedItem = Instance.GetRoomItemHandler().GetItem(packet.PopInt());

                if (selectedItem != null && !Instance.GetWired().OtherBoxHasItem(this, selectedItem.Id))
                    SetItems.TryAdd(selectedItem.Id, selectedItem);
            }

            StringData = movement + ";" + rotation;
            Delay = packet.PopInt();
        }

        public bool Execute(params object[] @params)
        {
            if (SetItems.Count == 0)
                return false;

            if (_next == 0 || _next < PlusEnvironment.Now())
                _next = PlusEnvironment.Now() + Delay;

            if (!_requested) {
                TickCount = Delay;
                _requested = true;
            }

            return true;
        }

        public bool OnCycle()
        {
            if (Instance == null || !_requested || _next == 0)
                return false;

            long now = PlusEnvironment.Now();
            if (_next < now) {
                foreach (Item item in SetItems.Values.ToList()) {
                    if (item == null)
                        continue;

                    if (!Instance.GetRoomItemHandler().GetFloor.Contains(item))
                        continue;

                    if (Instance.GetWired().OtherBoxHasItem(this, item.Id))
                        SetItems.TryRemove(item.Id, out Item _);

                    Point Point = HandleMovement(Convert.ToInt32(StringData.Split(';')[0]), new Point(item.GetX, item.GetY));
                    int newRot = HandleRotation(Convert.ToInt32(StringData.Split(';')[1]), item.Rotation);

                    Instance.GetWired().OnUserFurniCollision(Instance, item);

                    if (!Instance.GetGameMap().ItemCanMove(item, Point))
                        continue;

                    if (Instance.GetGameMap().CanRollItemHere(Point.X, Point.Y) && !Instance.GetGameMap().SquareHasUsers(Point.X, Point.Y)) {
                        double newZ = Instance.GetGameMap().GetHeightForSquareFromData(Point);
                        bool canBePlaced = true;

                        List<Item> items = Instance.GetGameMap().GetCoordinatedItems(Point);
                        foreach (Item IItem in items.ToList()) {
                            if (IItem == null || IItem.Id == item.Id)
                                continue;

                            if (!IItem.GetBaseItem().Walkable) {
                                _next = 0;
                                canBePlaced = false;
                                break;
                            }

                            if (IItem.TotalHeight > newZ)
                                newZ = IItem.TotalHeight;

                            if (canBePlaced && !IItem.GetBaseItem().Stackable)
                                canBePlaced = false;
                        }

                        if (newRot != item.Rotation) {
                            item.Rotation = newRot;
                            item.UpdateState(false, true);
                        }

                        if (canBePlaced && Point != item.Coordinate) {
                            Instance.SendPacket(new SlideObjectBundleComposer(item.GetX, item.GetY, item.GetZ, Point.X,
                                Point.Y, newZ, 0, 0, item.Id));
                            Instance.GetRoomItemHandler().SetFloorItem(item, Point.X, Point.Y, newZ);
                        }
                    }
                }

                _next = 0;
                return true;
            }

            return false;
        }

        private int HandleRotation(int mode, int rotation)
        {
            switch (mode) {
                case 1: {
                        rotation += 2;
                        if (rotation > 6) {
                            rotation = 0;
                        }

                        break;
                    }

                case 2: {
                        rotation -= 2;
                        if (rotation < 0) {
                            rotation = 6;
                        }

                        break;
                    }

                case 3: {
                        if (RandomNumber.GenerateRandom(0, 2) == 0) {
                            rotation += 2;
                            if (rotation > 6) {
                                rotation = 0;
                            }
                        } else {
                            rotation -= 2;
                            if (rotation < 0) {
                                rotation = 6;
                            }
                        }

                        break;
                    }
            }

            return rotation;
        }

        private Point HandleMovement(int mode, Point position)
        {
            Point newPos = new();
            switch (mode) {
                case 0: {
                        newPos = position;
                        break;
                    }
                case 1: {
                        switch (RandomNumber.GenerateRandom(1, 4)) {
                            case 1:
                                newPos = new Point(position.X + 1, position.Y);
                                break;
                            case 2:
                                newPos = new Point(position.X - 1, position.Y);
                                break;
                            case 3:
                                newPos = new Point(position.X, position.Y + 1);
                                break;
                            case 4:
                                newPos = new Point(position.X, position.Y - 1);
                                break;
                        }

                        break;
                    }
                case 2: {
                        if (RandomNumber.GenerateRandom(0, 2) == 1) {
                            newPos = new Point(position.X - 1, position.Y);
                        } else {
                            newPos = new Point(position.X + 1, position.Y);
                        }

                        break;
                    }
                case 3: {
                        if (RandomNumber.GenerateRandom(0, 2) == 1) {
                            newPos = new Point(position.X, position.Y - 1);
                        } else {
                            newPos = new Point(position.X, position.Y + 1);
                        }

                        break;
                    }
                case 4: {
                        newPos = new Point(position.X, position.Y - 1);
                        break;
                    }
                case 5: {
                        newPos = new Point(position.X + 1, position.Y);
                        break;
                    }
                case 6: {
                        newPos = new Point(position.X, position.Y + 1);
                        break;
                    }
                case 7: {
                        newPos = new Point(position.X - 1, position.Y);
                        break;
                    }
            }

            return newPos;
        }
    }
}