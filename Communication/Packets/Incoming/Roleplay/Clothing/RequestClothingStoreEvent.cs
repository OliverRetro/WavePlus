using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Roleplay.Clothing;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Clothing;

namespace Plus.Communication.Packets.Incoming.Roleplay.Clothing
{
    internal class RequestClothingStoreEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || !session.GetHabbo().InRoom)
                return;

            IReadOnlyList<RpClothingCategory> categories =
                PlusEnvironment.GetRpClothingStoreManager().GetCategoriesForRoom(session.GetHabbo().CurrentRoomId);

            session.SendPacket(new ClothingStoreComposer(categories));
        }
    }
}