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
                        db.users.Where(u => u.id == userId).ExecuteUpdate(s => s.SetProperty(u => u.credits, credits));

                    break;
                }

                case "pixels":
                case "duckets":
                {
                    int duckets = client.GetHabbo().Duckets;
                    using (WavePlusContext db = PlusEnvironment.GetDbContext())
                        db.users.Where(u => u.id == userId).ExecuteUpdate(s => s.SetProperty(u => u.activity_points, duckets));

                    break;
                }

                case "diamonds":
                {
                    int diamonds = client.GetHabbo().Diamonds;
                    using (WavePlusContext db = PlusEnvironment.GetDbContext())
                        db.users.Where(u => u.id == userId).ExecuteUpdate(s => s.SetProperty(u => u.vip_points, diamonds));

                    break;
                }

                case "gotw":
                {
                    int gotw = client.GetHabbo().GotwPoints;
                    using (WavePlusContext db = PlusEnvironment.GetDbContext())
                        db.users.Where(u => u.id == userId).ExecuteUpdate(s => s.SetProperty(u => u.gotw_points, gotw));

                    break;
                }
            }

            return true;
        }
    }
}