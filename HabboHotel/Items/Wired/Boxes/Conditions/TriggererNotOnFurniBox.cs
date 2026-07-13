using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Items.Wired.Boxes.Conditions
{
    internal class TriggererNotOnFurniBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.ConditionTriggererNotOnFurni;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public TriggererNotOnFurniBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket packet)
        {
            int unknown = packet.PopInt();
            string unknown2 = packet.PopString();

            if (SetItems.Count > 0)
                SetItems.Clear();

            int furniCount = packet.PopInt();
            for (int i = 0; i < furniCount; i++)
            {
                Item selectedItem = Instance.GetRoomItemHandler().GetItem(packet.PopInt());
                if (selectedItem != null)
                    SetItems.TryAdd(selectedItem.Id, selectedItem);
            }
        }

        public bool Execute(params object[] @params)
        {
            if (@params.Length == 0)
                return false;

            Habbo player = (Habbo)@params[0];

            RoomUser user = player?.CurrentRoom?.GetRoomUserManager().GetRoomUserByHabbo(player.Username);
            if (user == null)
                return false;

            List<Item> itemsOnSquare = Instance.GetGameMap().GetAllRoomItemForSquare(user.X, user.Y);
            foreach (Item item in itemsOnSquare.ToList())
            {
                if (SetItems.ContainsKey(item.Id))
                    return false;
            }

            return true;
        }
    }
}