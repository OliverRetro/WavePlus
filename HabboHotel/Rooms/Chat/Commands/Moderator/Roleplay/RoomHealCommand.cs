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
                if (rpStats.Health < UserRpStats.GetMaxHealth(roomUser.GetClient().GetHabbo()))
                {
                    if (rpStats.IsDead)
                        PlusEnvironment.GetHospitalManager().CancelDeath(roomUser.GetClient().GetHabbo());

                    rpStats.Heal(roomUser.GetClient().GetHabbo(), 200);
                }
            }

            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*" + session.GetHabbo().Username + " heals everyone in the room to their full HP!*", 0, 23));
        }
    }
}
