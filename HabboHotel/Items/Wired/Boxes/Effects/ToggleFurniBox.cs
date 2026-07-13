using System.Collections.Concurrent;
using System.Linq;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class ToggleFurniBox : IWiredItem, IWiredCycle
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.EffectToggleFurniState;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public int TickCount { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }

        public int Delay
        {
            get => _delay;
            set
            {
                _delay = value;
                TickCount = value;
            }
        }

        public string ItemsData { get; set; }

        private long _next;
        private int _delay;
        private bool _requested;

        public ToggleFurniBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket packet)
        {
            SetItems.Clear();
            int unknown = packet.PopInt();
            string unknown2 = packet.PopString();

            int furniCount = packet.PopInt();
            for (int i = 0; i < furniCount; i++)
            {
                Item selectedItem = Instance.GetRoomItemHandler().GetItem(packet.PopInt());
                if (selectedItem != null)
                    SetItems.TryAdd(selectedItem.Id, selectedItem);
            }

            int delay = packet.PopInt();
            Delay = delay;
        }

        public bool Execute(params object[] @params)
        {
            if (_next == 0 || _next < PlusEnvironment.Now())
                _next = PlusEnvironment.Now() + Delay;


            _requested = true;
            TickCount = Delay;
            return true;
        }

        public bool OnCycle()
        {
            if (SetItems.Count == 0 || !_requested)
                return false;

            long now = PlusEnvironment.Now();
            if (_next < now)
            {
                foreach (Item item in SetItems.Values.ToList())
                {
                    if (item == null)
                        continue;

                    if (!Instance.GetRoomItemHandler().GetFloor.Contains(item))
                    {
                        SetItems.TryRemove(item.Id, out Item _);
                        continue;
                    }

                    item.Interactor.OnWiredTrigger(item);
                }

                _requested = false;

                _next = 0;
                TickCount = Delay;
            }

            return true;
        }
    }
}