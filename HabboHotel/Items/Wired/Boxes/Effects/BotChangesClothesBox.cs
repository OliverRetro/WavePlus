using System.Collections.Concurrent;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Incoming;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.EF;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class BotChangesClothesBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.EffectBotChangesClothesBox;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public BotChangesClothesBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket packet)
        {
            int unknown = packet.PopInt();
            string botConfiguration = packet.PopString();

            if (SetItems.Count > 0)
                SetItems.Clear();

            StringData = botConfiguration;
        }

        public bool Execute(params object[] @params)
        {
            if (@params == null || @params.Length == 0)
                return false;

            if (string.IsNullOrEmpty(StringData))
                return false;

            string[] stuff = StringData.Split('\t');
            if (stuff.Length != 2)
                return false; //This is important, incase a cunt scripts.

            string username = stuff[0];

            RoomUser user = Instance.GetRoomUserManager().GetBotByName(username);
            if (user == null)
                return false;

            string figure = stuff[1];

            user.BotData.Look = figure;
            user.BotData.Gender = "M";

            MessageComposer userChangeComposer = new UserChangeComposer(user.VirtualId, user.BotData);
            Instance.SendPacket(userChangeComposer);

            uint botId = (uint)user.BotData.Id;
            string look = user.BotData.Look;
            string gender = user.BotData.Gender;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Bots.Where(b => b.Id == botId)
                    .ExecuteUpdate(s => s.SetProperty(b => b.Look, look).SetProperty(b => b.Gender, gender));
            }

            return true;
        }
    }
}