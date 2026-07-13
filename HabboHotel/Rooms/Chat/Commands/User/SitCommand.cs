using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class SitCommand : IChatCommand
    {
        public string PermissionRequired => "command_sit";

        public string Parameters => "";

        public string Description => "Allows you to sit down in your current spot.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session.GetHabbo().GetRpStats().IsDead)
            {
                session.SendWhisper("You are actively bleeding out. Please don't move.", 1);
                return;
            }

            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
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