using System;
using System.Collections.Concurrent;
using System.Linq;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items.Wired.Boxes.Conditions
{
    internal class FurniMatchStateAndPositionBox : IWiredItem
    {
        public Room Instance { get; set; }

        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.ConditionMatchStateAndPosition;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }

        public string StringData { get; set; }

        public bool BoolData { get; set; }

        public string ItemsData { get; set; }

        public FurniMatchStateAndPositionBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket packet)
        {
            if (SetItems.Count > 0)
                SetItems.Clear();

            int unknown = packet.PopInt();
            int state = packet.PopInt();
            int direction = packet.PopInt();
            int placement = packet.PopInt();
            string unknown2 = packet.PopString();

            int furniCount = packet.PopInt();
            for (int i = 0; i < furniCount; i++) {
                Item selectedItem = Instance.GetRoomItemHandler().GetItem(packet.PopInt());
                if (selectedItem != null)
                    SetItems.TryAdd(selectedItem.Id, selectedItem);
            }

            StringData = state + ";" + direction + ";" + placement;
        }

        public bool Execute(params object[] @params)
        {
            if (@params.Length == 0)
                return false;

            if (string.IsNullOrEmpty(StringData) || StringData == "0;0;0" || SetItems.Count == 0)
                return false;

            foreach (Item item in SetItems.Values.ToList()) {
                if (item == null)
                    continue;

                if (!Instance.GetRoomItemHandler().GetFloor.Contains(item))
                    continue;

                foreach (string I in ItemsData.Split(';')) {
                    if (string.IsNullOrEmpty(I))
                        continue;

                    Item ii = Instance.GetRoomItemHandler().GetItem(Convert.ToInt32(I.Split(':')[0]));
                    if (ii == null)
                        continue;

                    string[] partsString = I.Split(':');
                    string[] part = partsString[1].Split(',');

                    if (int.Parse(StringData.Split(';')[0]) == 1) //State
                    {
                        try {
                            if (ii.ExtraData != part[4])
                                return false;
                        }
                        catch {
                        }
                    }

                    if (int.Parse(StringData.Split(';')[1]) == 1) //Direction
                    {
                        try {
                            if (ii.Rotation != Convert.ToInt32(part[3]))
                                return false;
                        }
                        catch {
                        }
                    }

                    if (int.Parse(StringData.Split(';')[2]) == 1) //Position
                    {
                        try {
                            if (ii.GetX != Convert.ToInt32(part[0]) || ii.GetY != Convert.ToInt32(part[1]) ||
                                ii.GetZ != Convert.ToDouble(part[2]))
                                return false;
                        }
                        catch {
                        }
                    }
                }
            }

            return true;
        }
    }
}