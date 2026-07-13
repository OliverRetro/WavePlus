using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Rcon.Commands.User
{
    internal class SyncUserCurrencyCommand : IRconCommand
    {
        public string Description => "This command is used to sync a users specified currency to the database.";

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

            switch (currency)
            {
                default:
                    return false;

                case "coins":
                case "credits":
                    {
                        int credits = client.GetHabbo().Credits;
                        using (WavePlusContext db = PlusEnvironment.GetDbContext())
                            db.Users.Where(u => u.Id == userId).ExecuteUpdate(s => s.SetProperty(u => u.Credits, credits));

                        break;
                    }

                case "pixels":
                case "duckets":
                    {
                        int duckets = client.GetHabbo().Duckets;
                        using (WavePlusContext db = PlusEnvironment.GetDbContext())
                            db.Users.Where(u => u.Id == userId).ExecuteUpdate(s => s.SetProperty(u => u.ActivityPoints, duckets));

                        break;
                    }

                case "diamonds":
                    {
                        int diamonds = client.GetHabbo().Diamonds;
                        using (WavePlusContext db = PlusEnvironment.GetDbContext())
                            db.Users.Where(u => u.Id == userId).ExecuteUpdate(s => s.SetProperty(u => u.VipPoints, diamonds));

                        break;
                    }

                case "gotw":
                    {
                        int gotw = client.GetHabbo().GotwPoints;
                        using (WavePlusContext db = PlusEnvironment.GetDbContext())
                            db.Users.Where(u => u.Id == userId).ExecuteUpdate(s => s.SetProperty(u => u.GotwPoints, gotw));

                        break;
                    }
            }

            return true;
        }
    }
}