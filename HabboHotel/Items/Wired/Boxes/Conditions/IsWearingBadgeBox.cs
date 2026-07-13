using System.Collections.Concurrent;
using System.Linq;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Badges;

namespace Plus.HabboHotel.Items.Wired.Boxes.Conditions
{
    internal class IsWearingBadgeBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.ConditionIsWearingBadge;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public IsWearingBadgeBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket packet)
        {
            int unknown = packet.PopInt();
            string badgeCode = packet.PopString();

            StringData = badgeCode;
        }

        public bool Execute(params object[] @params)
        {
            if (@params.Length == 0)
                return false;

            if (string.IsNullOrEmpty(StringData))
                return false;

            Habbo player = (Habbo)@params[0];
            if (player == null)
                return false;

            if (!player.GetBadgeComponent().GetBadges().Contains(player.GetBadgeComponent().GetBadge(StringData)))
                return false;

            foreach (Badge badge in player.GetBadgeComponent().GetBadges().ToList())
            {
                if (badge.Slot <= 0)
                    continue;

                if (badge.Code == StringData)
                    return true;
            }

            return false;
        }
    }
}