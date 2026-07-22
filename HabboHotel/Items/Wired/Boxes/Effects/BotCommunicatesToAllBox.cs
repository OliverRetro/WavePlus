using System.Collections.Concurrent;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class BotCommunicatesToAllBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.EffectBotCommunicatesToAllBox;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public BotCommunicatesToAllBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket packet)
        {
            int unknown = packet.PopInt();
            int chatMode = packet.PopInt();
            string chatConfig = packet.PopString();

            if (SetItems.Count > 0)
                SetItems.Clear();

            //this.StringData = ChatConfig.Replace('\t', ';') + ";" + ChatMode;
        }

        public bool Execute(params object[] @params)
        {
            if (@params == null || @params.Length == 0)
                return false;

            if (string.IsNullOrEmpty(StringData))
                return false;

            RoomUser user = Instance.GetRoomUserManager().GetBotByName(StringData);
            if (user == null)
                return false;

            //TODO: This needs finishing.

            return true;
        }
    }
}