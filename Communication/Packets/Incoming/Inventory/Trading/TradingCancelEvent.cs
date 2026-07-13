using Plus.Communication.Packets.Outgoing.Inventory.Trading;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Trading;

namespace Plus.Communication.Packets.Incoming.Inventory.Trading
{
    internal class TradingCancelEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() == null || !session.GetHabbo().InRoom)
                return;

            Room room = session.GetHabbo().CurrentRoom;

            RoomUser roomUser = room?.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (roomUser == null)
                return;

            if (!room.GetTrading().TryGetTrade(roomUser.TradeId, out Trade trade))
            {
                session.SendPacket(new TradingClosedComposer(session.GetHabbo().Id));
                return;
            }

            trade.EndTrade(session.GetHabbo().Id);
        }
    }
}