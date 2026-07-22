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

            // Only drop the click when it is a genuine no-op. user.X/Y is not updated until the
            // pending step COMMITS 500ms later, so while a step is in flight it still reads as the
            // tile being left - and clicking that tile back is a reversal, not a repeat. Comparing
            // blindly is what made one-tile back-and-forth clicking swallow every other click,
            // depending purely on where in the step cadence the click landed.
            if (moveX == user.X && moveY == user.Y && !user.SetStep && !user.IsWalking)
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