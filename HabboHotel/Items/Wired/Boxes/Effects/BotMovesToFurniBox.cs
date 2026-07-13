using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class BotMovesToFurniBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.EffectBotMovesToFurniBox;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public BotMovesToFurniBox(Room instance, Item item)
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
            if (@params == null || @params.Length == 0 || string.IsNullOrEmpty(StringData))
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

            if (user.IsWalking)
            {
                user.ClearMovement(true);
            }

            user.BotData.ForcedMovement = true;
            user.BotData.TargetCoordinate = new Point(item.GetX, item.GetY);
            user.MoveTo(item.GetX, item.GetY);

            return true;
        }
    }
}