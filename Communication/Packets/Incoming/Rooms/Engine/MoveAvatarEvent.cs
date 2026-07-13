using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    internal class MoveAvatarEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null)
                return;

            if (!session.GetHabbo().InRoom)
                return;

            Room room = session.GetHabbo().CurrentRoom;

            RoomUser user = room?.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null || !user.CanWalk)
                return;

            int moveX = packet.PopInt();
            int moveY = packet.PopInt();

            if (moveX == user.X && moveY == user.Y)
                return;

            if (user.RidingHorse)
            {
                RoomUser horse = room.GetRoomUserManager().GetRoomUserByVirtualId(user.HorseId);
                horse?.MoveTo(moveX, moveY);
            }

            user.MoveTo(moveX, moveY);
        }
    }
}