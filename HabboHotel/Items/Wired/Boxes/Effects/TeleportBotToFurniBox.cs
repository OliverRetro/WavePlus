using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class TeleportBotToFurniBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.EffectTeleportBotToFurniBox;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public TeleportBotToFurniBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket packet)
        {
            int unknown = packet.PopInt();
            string botName = packet.PopString();

            if (SetItems.Count > 0)
                SetItems.Clear();

            int furniCount = packet.PopInt();
            for (int i = 0; i < furniCount; i++)
            {
                Item selectedItem = Instance.GetRoomItemHandler().GetItem(packet.PopInt());
                if (selectedItem != null)
                    SetItems.TryAdd(selectedItem.Id, selectedItem);
            }

            StringData = botName;
        }

        public bool Execute(params object[] @params)
        {
            if (string.IsNullOrEmpty(StringData))
                return false;

            RoomUser user = Instance.GetRoomUserManager().GetBotByName(StringData);
            if (user == null)
                return false;

            Random rand = new();
            List<Item> items = SetItems.Values.ToList();
            items = items.OrderBy(x => rand.Next()).ToList();

            if (items.Count == 0)
                return false;

            Item item = items.First();
            if (item == null)
                return false;

            if (!Instance.GetRoomItemHandler().GetFloor.Contains(item))
            {
                SetItems.TryRemove(item.Id, out item);

                if (items.Contains(item))
                    items.Remove(item);

                if (SetItems.Count == 0 || items.Count == 0)
                    return false;

                item = items.First();
                if (item == null)
                    return false;
            }

            if (Instance.GetGameMap() == null)
                return false;

            Instance.GetGameMap().TeleportToItem(user, item);
            Instance.GetRoomUserManager().UpdateUserStatusses();


            return true;
        }
    }
}