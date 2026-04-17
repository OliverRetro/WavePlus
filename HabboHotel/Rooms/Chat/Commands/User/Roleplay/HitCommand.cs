using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class HitCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_hit";

        public string Parameters => "%username%";

        public string Description => "Attack another user.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1)
            {
                session.SendWhisper("Please enter the username of the user you wish attack.");
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);

            if (targetClient == null)
            {
                session.SendWhisper("The user could not be found.", 1);
                return; 
            }

            if (targetClient == session)
            {
                session.SendWhisper("You cannot hit yourself!", 1);
                return;
            }

            if (targetClient.GetHabbo().CurrentRoomId != session.GetHabbo().CurrentRoomId)
            {
                session.SendWhisper("The user is not in the same room as you.", 1);
                return;
            }
                
            PlusEnvironment.GetCombatManager().AttackUser(session.GetHabbo(), targetClient.GetHabbo());      
        }
    }
}