using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Inventory.Trading;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Trading;

namespace Plus.Communication.Packets.Incoming.Inventory.Trading
{
    internal class InitTradeEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int userId = packet.PopInt();

            if (session == null || session.GetHabbo() == null || !session.GetHabbo().InRoom)
                return;

            Room room = session.GetHabbo().CurrentRoom;

            RoomUser roomUser = room?.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (roomUser == null)
                return;

            RoomUser targetUser = room.GetRoomUserManager().GetRoomUserByVirtualId(userId);
            if (targetUser == null)
                return;

            if (session.GetHabbo().TradingLockExpiry > 0) {
                if (session.GetHabbo().TradingLockExpiry > PlusEnvironment.GetUnixTimestamp()) {
                    session.SendNotification("You're currently banned from trading.");
                    return;
                }

                session.GetHabbo().TradingLockExpiry = 0;
                session.SendNotification("Your trading ban has now expired.");

                // NOTE: legacy SQL used WHERE `id`, but user_info has no `id` column; the key is `user_id`.
                int tradeUserId = session.GetHabbo().Id;
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    db.UserInfos.Where(u => u.UserId == tradeUserId).ExecuteUpdate(s => s.SetProperty(u => u.TradingLocked, 0d));
                }
            }

            if (!session.GetHabbo().GetPermissions().HasRight("room_trade_override")) {
                if (room.TradeSettings == 0) {
                    session.SendPacket(new TradingErrorComposer(6, targetUser.GetUsername()));
                    return;
                }

                if (room.TradeSettings == 1 && room.OwnerId != session.GetHabbo().Id) {
                    session.SendPacket(new TradingErrorComposer(6, targetUser.GetUsername()));
                    return;
                }
            }

            if (roomUser.IsTrading && roomUser.TradePartner != targetUser.UserId) {
                session.SendPacket(new TradingErrorComposer(7, targetUser.GetUsername()));
                return;
            }

            if (targetUser.IsTrading && targetUser.TradePartner != roomUser.UserId) {
                session.SendPacket(new TradingErrorComposer(8, targetUser.GetUsername()));
                return;
            }

            if (!targetUser.GetClient().GetHabbo().AllowTradingRequests) {
                session.SendPacket(new TradingErrorComposer(4, targetUser.GetUsername()));
                return;
            }

            // Furniture-trading setting: a user who disabled it can neither initiate nor be traded.
            if (session.GetHabbo().GetRpSettings()?.FurnitureTrading == false) {
                session.SendWhisper("You have furniture trading disabled in your settings.", 1);
                return;
            }

            if (targetUser.GetClient().GetHabbo().GetRpSettings()?.FurnitureTrading == false) {
                session.SendPacket(new TradingErrorComposer(4, targetUser.GetUsername()));
                return;
            }

            if (targetUser.GetClient().GetHabbo().TradingLockExpiry > 0) {
                session.SendPacket(new TradingErrorComposer(4, targetUser.GetUsername()));
                return;
            }

            if (!room.GetTrading().StartTrade(roomUser, targetUser, out Trade trade)) {
                session.SendNotification("An error occured trying to start this trade");
                return;
            }

            if (targetUser.HasStatus("trd"))
                targetUser.RemoveStatus("trd");
            if (roomUser.HasStatus("trd"))
                roomUser.RemoveStatus("trd");

            targetUser.SetStatus("trd");
            targetUser.UpdateNeeded = true;
            roomUser.SetStatus("trd");
            roomUser.UpdateNeeded = true;

            trade.SendPacket(new TradingStartComposer(roomUser.UserId, targetUser.UserId));
        }
    }
}