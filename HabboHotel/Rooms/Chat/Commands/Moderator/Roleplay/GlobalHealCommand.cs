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
            if (@params.Length == 1)
            {
                session.SendWhisper("With global heal, you need to choose the amount to heal, max is 100.");
                return;
            }

            if (!int.TryParse(@params[1], out int amount))
            {
                session.SendWhisper("Please choose an valid number between 0-100.");
                return;
            }

            if (amount > 100 || amount < 0)
            {
                session.SendWhisper("Please choose an valid number between 0-100.");
                return;
            }

            foreach (RoomUser user in PlusEnvironment.GetGame().GetRoomManager().GetAllRoomUsers())
            {
                if (user.IsBot)
                    continue;

                GameClient client = user.GetClient();
                if (client == null)
                    continue;

                if(client.GetHabbo().GetRpStats().Health < 100) {
                    client.GetHabbo().GetRpStats().Heal(amount);
                    session.SendWhisper("You were healed!", 1);
                }

                session.SendWhisper("Everyone in a room has been healed!", 1);
            }
        }
    }
}