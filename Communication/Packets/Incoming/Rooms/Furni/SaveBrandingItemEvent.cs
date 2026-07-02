using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Furni
{
    internal class SaveBrandingItemEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().InRoom)
                return;

            Room room = session.GetHabbo().CurrentRoom;
            if (room == null)
                return;

            if (!room.CheckRights(session, true) || !session.GetHabbo().GetPermissions().HasRight("room_item_save_branding_items"))
                return;

            int itemId = packet.PopInt();
            Item item = room.GetRoomItemHandler().GetItem(itemId);
            if (item == null)
                return;

            if (item.Data.InteractionType == InteractionType.Background) {
                // Client sends: [int: count = pairs*2], then [string: key][string: value] × (count/2)
                // Store as "key=value;key=value;" to match the Java InteractionCustomValues format.
                int count = packet.PopInt();
                var sb = new System.Text.StringBuilder();
                for (int i = 0; i < count / 2; i++)
                {
                    string key = packet.PopString();
                    string value = packet.PopString();
                    sb.Append(key).Append('=').Append(value).Append(';');
                }
                item.ExtraData = sb.ToString();
            } else if (item.Data.InteractionType == InteractionType.FxProvider) {
                /*int Unknown = Packet.PopInt();
                string Data = Packet.PopString();
                int EffectId = Packet.PopInt();

                Item.ExtraData = Convert.ToString(EffectId);*/
            }

            room.GetRoomItemHandler().SetFloorItem(session, item, item.GetX, item.GetY, item.Rotation, false, false, true);
        }
    }
}