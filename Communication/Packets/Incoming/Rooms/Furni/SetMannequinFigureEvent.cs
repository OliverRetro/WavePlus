using System;
using System.Linq;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    internal class SetMannequinFigureEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            Room room = session.GetHabbo().CurrentRoom;
            if (room == null || !room.CheckRights(session, true))
                return;

            int itemId = packet.PopInt();
            Item item = session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetItem(itemId);
            if (item == null)
                return;

            string gender = session.GetHabbo().Gender.ToLower();
            string figure = session.GetHabbo().Look.Split('.').Where(str => !str.Contains("hr") && !str.Contains("hd") && !str.Contains("he") && !str.Contains("ea") && !str.Contains("pt")).Aggregate("", (current, str) => current + (str + "."));

            figure = figure.TrimEnd('.');
            if (item.ExtraData.Contains(Convert.ToChar(5))) {
                string[] flags = item.ExtraData.Split(Convert.ToChar(5));
                item.ExtraData = gender + Convert.ToChar(5) + figure + Convert.ToChar(5) + flags[2];
            }
            else
                item.ExtraData = gender + Convert.ToChar(5) + figure + Convert.ToChar(5) + "Default";

            item.UpdateState(true, true);
        }
    }
}