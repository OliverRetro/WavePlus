using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Rcon.Commands.User
{
    // Grants timed VIP to an online user. Intended for the web store / payment callback.
    // Usage: give_user_vip %userId% %seconds%
    internal class GiveUserVipCommand : IRconCommand
    {
        public string Description => "Grants (or extends) a user's timed VIP by a number of seconds.";

        public string Parameters => "%userId% %seconds%";

        public bool TryExecute(string[] parameters)
        {
            if (parameters.Length < 2)
                return false;

            if (!int.TryParse(parameters[0], out int userId) || !int.TryParse(parameters[1], out int seconds))
                return false;

            if (seconds <= 0)
                return false;

            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
            if (client?.GetHabbo() == null)
                return false;

            client.GetHabbo().GrantVip(seconds);
            return true;
        }
    }
}