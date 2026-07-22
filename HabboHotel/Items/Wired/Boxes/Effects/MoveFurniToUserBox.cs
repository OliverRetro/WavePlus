using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Plus.Communication.Packets.Incoming;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class MoveFurniToUserBox : IWiredItem, IWiredCycle
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectMoveFurniToNearestUser;

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

        public MoveFurniToUserBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();
            TickCount = Delay;
            _requested = false;
        }

        public void HandleSave(ClientPacket packet)
        {
            int unknown = packet.PopInt();
            string unknown2 = packet.PopString();

            if (SetItems.Count > 0)
                SetItems.Clear();

            int furniCount = packet.PopInt();
            for (int i = 0; i < furniCount; i++) {
                Item selectedItem = Instance.GetRoomItemHandler().GetItem(packet.PopInt());

                if (selectedItem != null && !Instance.GetWired().OtherBoxHasItem(this, selectedItem.Id))
                    SetItems.TryAdd(selectedItem.Id, selectedItem);
            }

            int delay = packet.PopInt();
            Delay = delay;
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
                foreach (Item Item in SetItems.Values.ToList()) {
                    if (Item == null)
                        continue;

                    if (!Instance.GetRoomItemHandler().GetFloor.Contains(Item))
                        continue;

                    if (Instance.GetWired().OtherBoxHasItem(this, Item.Id))
                        SetItems.TryRemove(Item.Id, out Item _);

                    Point point = Instance.GetGameMap().GetChaseMovement(Item);

                    Instance.GetWired().OnUserFurniCollision(Instance, Item);

                    if (!Instance.GetGameMap().ItemCanMove(Item, point))
                        continue;

                    if (Instance.GetGameMap().CanRollItemHere(point.X, point.Y) && !Instance.GetGameMap().SquareHasUsers(point.X, point.Y)) {
                        double newZ = Item.GetZ;
                        bool canBePlaced = true;

                        List<Item> items = Instance.GetGameMap().GetCoordinatedItems(point);
                        foreach (Item IItem in items.ToList()) {
                            if (IItem == null || IItem.Id == Item.Id)
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

                        if (canBePlaced && point != Item.Coordinate) {
                            Instance.SendPacket(new SlideObjectBundleComposer(Item.GetX, Item.GetY, Item.GetZ, point.X,
                                point.Y, newZ, 0, 0, Item.Id));
                            Instance.GetRoomItemHandler().SetFloorItem(Item, point.X, point.Y, newZ);
                        }
                    }
                }

                _next = 0;
                return true;
            }

            return false;
        }
    }
}