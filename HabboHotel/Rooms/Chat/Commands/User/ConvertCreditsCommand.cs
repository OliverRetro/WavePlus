using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database.EF;
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

            try {
                int userId = session.GetHabbo().Id;

                List<uint> itemIds;
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    // NOTE: legacy query also matched room_id = '' which cannot occur for a uint column; kept room_id == 0 only.
                    itemIds = db.Items.Where(i => i.UserId == userId && i.RoomId == 0).Select(i => i.Id).ToList();
                }

                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    foreach (uint id in itemIds) {
                        Item item = session.GetHabbo().GetInventoryComponent().GetItem(Convert.ToInt32(id));
                        if (item == null || item.RoomId > 0 || item.Data.InteractionType != InteractionType.Exchange)
                            continue;

                        int value = item.Data.BehaviourData;

                        uint itemId = (uint)item.Id;
                        db.Items.Where(i => i.Id == itemId).ExecuteDelete();

                        session.GetHabbo().GetInventoryComponent().RemoveItem(item.Id);

                        totalValue += value;

                        if (value > 0) {
                            session.GetHabbo().Credits += value;
                            session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
                        }
                    }
                }

                if (totalValue > 0)
                    session.SendNotification("All credits have successfully been converted!\r\r(Total value: " + totalValue + " credits!");
                else
                    session.SendNotification("It appears you don't have any exchangeable items!");
            } catch {
                session.SendNotification("Oops, an error occoured whilst converting your credits!");
            }
        }
    }
}