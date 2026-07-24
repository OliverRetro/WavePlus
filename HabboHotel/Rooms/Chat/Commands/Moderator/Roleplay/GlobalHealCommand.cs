using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Roleplay;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Roleplay
{
    internal class GlobalHealCommand : IChatCommand
    {
        public string PermissionRequired => "command_global_heal";

        public string Parameters => "%amount%";

        public string Description => "Give everyone online HP!";

        // Heals cancel death, so they're useless if death blocks them.
        public bool UsableWhileDead => true;

        public bool UsableWhileCuffed => true;

        public void Execute(GameClient session, Room room, string[] @params)
        {
            foreach (RoomUser user in PlusEnvironment.GetGame().GetRoomManager().GetAllRoomUsers()) {
                if (user.IsBot)
                    continue;

                GameClient client = user.GetClient();
                if (client == null)
                    continue;

                if (client.GetHabbo().GetRpStats().Health < UserRpStats.GetMaxHealth(client.GetHabbo())) {
                    if (client.GetHabbo().GetRpStats().IsDead)
                        PlusEnvironment.GetHospitalManager().CancelDeath(client.GetHabbo());

                    client.GetHabbo().GetRpStats().Heal(client.GetHabbo(), 200);
                    client.SendWhisper("You were healed!", 1);
                }
            }

            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*heals everyone to their full HP!*", 0, 23, isRpAction: true));
        }
    }
}