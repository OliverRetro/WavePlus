using System;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Rcon.Commands.User
{
    internal class ReloadUserCurrencyCommand : IRconCommand
    {
        public string Description => "This command is used to update the users currency from the database.";

        public string Parameters => "%userId% %currency%";

        public bool TryExecute(string[] parameters)
        {
            if (!int.TryParse(parameters[0], out int userId))
                return false;

            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
            if (client == null || client.GetHabbo() == null)
                return false;

            // Validate the currency type
            if (string.IsNullOrEmpty(Convert.ToString(parameters[1])))
                return false;

            string currency = Convert.ToString(parameters[1]);

            switch (currency) {
                default:
                    return false;

                case "coins":
                case "credits": {
                        int credits;
                        using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                            credits = db.Users.Where(u => u.Id == userId).Select(u => u.Credits).FirstOrDefault() ?? 0;
                        }

                        client.GetHabbo().Credits = credits;
                        client.SendPacket(new CreditBalanceComposer(client.GetHabbo().Credits));
                        break;
                    }

                case "pixels":
                case "duckets": {
                        int duckets;
                        using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                            duckets = db.Users.Where(u => u.Id == userId).Select(u => u.ActivityPoints).FirstOrDefault() ?? 0;
                        }

                        client.GetHabbo().Duckets = duckets;
                        client.SendPacket(new HabboActivityPointNotificationComposer(client.GetHabbo().Duckets, duckets));
                        break;
                    }

                case "diamonds": {
                        int diamonds;
                        using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                            diamonds = db.Users.Where(u => u.Id == userId).Select(u => u.VipPoints).FirstOrDefault() ?? 0;
                        }

                        client.GetHabbo().Diamonds = diamonds;
                        client.SendPacket(new HabboActivityPointNotificationComposer(diamonds, 0, 5));
                        break;
                    }

                case "gotw": {
                        int gotw;
                        using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                            gotw = db.Users.Where(u => u.Id == userId).Select(u => u.GotwPoints).FirstOrDefault() ?? 0;
                        }

                        client.GetHabbo().GotwPoints = gotw;
                        client.SendPacket(new HabboActivityPointNotificationComposer(gotw, 0, 103));
                        break;
                    }
            }

            return true;
        }
    }
}