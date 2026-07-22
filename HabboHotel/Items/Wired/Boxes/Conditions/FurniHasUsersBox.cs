using System.Collections.Concurrent;
using System.Linq;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.PathFinding;

namespace Plus.HabboHotel.Items.Wired.Boxes.Conditions
{
    internal class FurniHasUsersBox : IWiredItem
    {
        public Room Instance { get; set; }

        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.ConditionFurniHasUsers;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }

        public string StringData { get; set; }

        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public FurniHasUsersBox(Room instance, Item item)
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
            for (int i = 0; i < furniCount; i++) {
                Item selectedItem = Instance.GetRoomItemHandler().GetItem(packet.PopInt());
                if (selectedItem != null)
                    SetItems.TryAdd(selectedItem.Id, selectedItem);
            }
        }

        public bool Execute(params object[] @params)
        {
            foreach (Item item in SetItems.Values.ToList()) {
                if (item == null || !Instance.GetRoomItemHandler().GetFloor.Contains(item))
                    continue;

                bool hasUsers = false;
                foreach (ThreeDCoord tile in item.GetAffectedTiles.Values) {
                    if (Instance.GetGameMap().SquareHasUsers(tile.X, tile.Y))
                        hasUsers = true;
                }

                if (Instance.GetGameMap().SquareHasUsers(item.GetX, item.GetY))
                    hasUsers = true;

                if (!hasUsers)
                    return false;
            }

            return true;
        }
    }
}