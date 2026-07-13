using System.Collections.Concurrent;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Items.Wired.Boxes.Conditions
{
    internal class ActorHasHandItemBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.ConditionActorHasHandItemBox;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public ActorHasHandItemBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;

            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket packet)
        {
            int unknown = packet.PopInt();
            int unknown2 = packet.PopInt();

            StringData = unknown2.ToString();
        }

        public bool Execute(params object[] @params)
        {
            if (@params.Length == 0 || Instance == null || string.IsNullOrEmpty(StringData))
                return false;

            Habbo player = (Habbo)@params[0];
            if (player == null)
                return false;

            RoomUser user = Instance.GetRoomUserManager().GetRoomUserByHabbo(player.Id);
            if (user == null)
                return false;

            if (user.CarryItemId != int.Parse(StringData))
                return false;

            return true;
        }
    }
}