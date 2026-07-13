using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class ForceSitCommand : IChatCommand
    {
        public string PermissionRequired => "command_forcesit";

        public string Parameters => "%username%";

        public string Description => "Force another to user sit.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length == 1)
            {
                session.SendWhisper("Oops, you forgot to choose a target user!");
                return;
            }

            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(@params[1]);
            if (user == null)
                return;

            if (user.Statusses.ContainsKey("lie") || user.IsLying || user.RidingHorse || user.IsWalking)
                return;

            if (!user.Statusses.ContainsKey("sit"))
            {
                if ((user.RotBody % 2) == 0)
                {
                    try
                    {
                        user.Statusses.Add("sit", "1.0");
                        user.Z -= 0.35;
                        user.IsSitting = true;
                        user.UpdateNeeded = true;
                    }
                    catch
                    {
                    }
                }
                else
                {
                    user.RotBody--;
                    user.Statusses.Add("sit", "1.0");
                    user.Z -= 0.35;
                    user.IsSitting = true;
                    user.UpdateNeeded = true;
                }
            }
            else if (user.IsSitting)
            {
                user.Z += 0.35;
                user.Statusses.Remove("sit");
                user.Statusses.Remove("1.0");
                user.IsSitting = false;
                user.UpdateNeeded = true;
            }
        }
    }
}