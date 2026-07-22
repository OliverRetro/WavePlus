using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    internal class MoveWallItemEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out Room room))
                return;

            if (!room.CheckRights(session))
                return;

            int itemId = packet.PopInt();
            string wallPositionData = packet.PopString();

            Item item = room.GetRoomItemHandler().GetItem(itemId);

            if (item == null)
                return;

            try {
                string wallPos = RoomItemHandling.WallPositionCheck(":" + wallPositionData.Split(':')[1]);
                item.WallCoord = wallPos;
            } catch {
                return;
            }

            room.GetRoomItemHandler().UpdateItem(item);
            room.SendPacket(new ItemUpdateComposer(item, room.OwnerId));
        }
    }
}