using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    internal class SetMannequinNameEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            Room room = session.GetHabbo().CurrentRoom;
            if (room == null || !room.CheckRights(session, true))
                return;

            int itemId = packet.PopInt();
            string name = packet.PopString();

            Item item = session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetItem(itemId);
            if (item == null)
                return;

            if (item.ExtraData.Contains(Convert.ToChar(5))) {
                string[] flags = item.ExtraData.Split(Convert.ToChar(5));
                item.ExtraData = flags[0] + Convert.ToChar(5) + flags[1] + Convert.ToChar(5) + name;
            } else
                item.ExtraData = "m" + Convert.ToChar(5) + ".ch-210-1321.lg-285-92" + Convert.ToChar(5) + "Default Mannequin";

            uint itemDbId = (uint)item.Id;
            string extraData = item.ExtraData;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Items.Where(i => i.Id == itemDbId).ExecuteUpdate(s => s.SetProperty(i => i.ExtraData, extraData));
            }

            item.UpdateState(true, true);
        }
    }
}