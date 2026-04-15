using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class GlobalHealCommand : IChatCommand
    {
        public string PermissionRequired => "command_global_heal";

        public string Parameters => "%amount%";

        public string Description => "Give everyone online HP!";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            foreach (RoomUser user in PlusEnvironment.GetGame().GetRoomManager().GetAllRoomUsers())
            {
                if (user.IsBot)
                    continue;

                GameClient client = user.GetClient();
                if (client == null)
                    continue;

                if (client.GetHabbo().GetRpStats().Health < client.GetHabbo().GetRpStats().GetMaxHealth(client.GetHabbo()))
                {
                    client.GetHabbo().GetRpStats().Heal(client.GetHabbo(), 200);
                    client.SendWhisper("You were healed!", 1);
                }
            }

            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*Heals everyone to their full HP!*", 0, 23));
        }
    }
}
