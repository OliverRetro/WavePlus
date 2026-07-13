using System.Collections.Concurrent;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class SetRollerSpeedBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.EffectSetRollerSpeed;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public SetRollerSpeedBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();

            if (SetItems.Count > 0)
                SetItems.Clear();
        }

        public void HandleSave(ClientPacket packet)
        {
            if (SetItems.Count > 0)
                SetItems.Clear();

            int unknown = packet.PopInt();
            string message = packet.PopString();

            StringData = message;

            if (!int.TryParse(StringData, out int speed))
            {
                StringData = "";
            }
        }

        public bool Execute(params object[] @params)
        {
            if (int.TryParse(StringData, out int speed))
            {
                Instance.GetRoomItemHandler().SetSpeed(speed);
            }

            return true;
        }
    }
}