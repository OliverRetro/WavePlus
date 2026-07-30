using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Rcon.Commands.User
{
    internal class TakeUserCurrencyCommand : IRconCommand
    {
        public string Description => "This command is used to take a specified amount of a specified currency from a user.";

        public string Parameters => "%userId% %currency% %amount%";

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

            if (!int.TryParse(parameters[2], out int amount))
                return false;

            switch (currency) {
                default:
                    return false;

                case "coins":
                case "credits": {
                        client.GetHabbo().Credits -= amount;

                        int credits = client.GetHabbo().Credits;
                        using (WavePlusContext db = PlusEnvironment.GetDbContext())
                            db.Users.Where(u => u.Id == userId).ExecuteUpdate(s => s.SetProperty(u => u.Credits, credits));

                        client.SendPacket(new CreditBalanceComposer(client.GetHabbo().Credits));
                        break;
                    }

                case "pixels":
                case "duckets": {
                        client.GetHabbo().Duckets -= amount;

                        int duckets = client.GetHabbo().Duckets;
                        using (WavePlusContext db = PlusEnvironment.GetDbContext())
                            db.Users.Where(u => u.Id == userId).ExecuteUpdate(s => s.SetProperty(u => u.ActivityPoints, duckets));

                        client.SendPacket(new HabboActivityPointNotificationComposer(client.GetHabbo().Duckets, amount));
                        break;
                    }

                case "diamonds": {
                        client.GetHabbo().Diamonds -= amount;

                        int diamonds = client.GetHabbo().Diamonds;
                        using (WavePlusContext db = PlusEnvironment.GetDbContext())
                            db.Users.Where(u => u.Id == userId).ExecuteUpdate(s => s.SetProperty(u => u.VipPoints, diamonds));

                        client.SendPacket(new HabboActivityPointNotificationComposer(client.GetHabbo().Diamonds, 0, 5));
                        break;
                    }

                case "gotw": {
                        client.GetHabbo().GotwPoints -= amount;

                        int gotw = client.GetHabbo().GotwPoints;
                        using (WavePlusContext db = PlusEnvironment.GetDbContext())
                            db.Users.Where(u => u.Id == userId).ExecuteUpdate(s => s.SetProperty(u => u.GotwPoints, gotw));

                        client.SendPacket(new HabboActivityPointNotificationComposer(client.GetHabbo().GotwPoints, 0, 103));
                        break;
                    }
            }

            return true;
        }
    }
}