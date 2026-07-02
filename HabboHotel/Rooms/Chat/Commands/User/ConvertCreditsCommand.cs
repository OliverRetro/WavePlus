using System;
using System.Data;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database.Interfaces;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class ConvertCreditsCommand : IChatCommand
    {
        public string PermissionRequired => "command_convert_credits";

        public string Parameters => "";

        public string Description => "Convert your exchangeable furniture into actual credits.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            int totalValue = 0;

            try
            {
                DataTable table;
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `id` FROM `items` WHERE `user_id` = '" + session.GetHabbo().Id + "' AND (`room_id`=  '0' OR `room_id` = '')");
                    table = dbClient.GetTable();
                }

                if (table == null) {
                    session.SendWhisper("You currently have no items in your inventory!");
                    return;
                }

                if (table.Rows.Count > 0) {
                    using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                    {
                        foreach (DataRow row in table.Rows)
                        {
                            Item item = session.GetHabbo().GetInventoryComponent().GetItem(Convert.ToInt32(row[0]));
                            if (item == null || item.RoomId > 0 || item.Data.InteractionType != InteractionType.Exchange)
                                continue;

                            int value = item.Data.BehaviourData;

                            dbClient.RunQuery("DELETE FROM `items` WHERE `id` = '" + item.Id + "' LIMIT 1");

                            session.GetHabbo().GetInventoryComponent().RemoveItem(item.Id);

                            totalValue += value;

                            if (value > 0) {
                                session.GetHabbo().Credits += value;
                                session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
                            }
                        }
                    }
                }

                if (totalValue > 0)
                    session.SendNotification("All credits have successfully been converted!\r\r(Total value: " + totalValue + " credits!");
                else
                    session.SendNotification("It appears you don't have any exchangeable items!");
            }
            catch
            {
                session.SendNotification("Oops, an error occoured whilst converting your credits!");
            }
        }
    }
}