using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class RoomHealCommand : IChatCommand
    {
        public string PermissionRequired => "command_room_heal";

        public string Parameters => "%amount%";

        public string Description => "Heals the room.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            foreach (RoomUser roomUser in room.GetRoomUserManager().GetRoomUsers())
            {
                if (roomUser == null || roomUser.GetClient() == null)
                    continue;

                UserRpStats rpStats = roomUser.GetClient().GetHabbo().GetRpStats();
                if (rpStats.Health < 100) {
                    rpStats.Heal(200);
                }
            }

            session.SendPacket(new ShoutComposer(session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*Heals everyone in the room to their full HP!*", 0, 23));
        }
    }
}