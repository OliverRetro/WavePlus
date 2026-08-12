using Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Roleplay.Clothing;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Clothing;
using System.Collections.Generic;
using System.Linq;

namespace Plus.Communication.Packets.Incoming.Roleplay.Clothing
{
    internal class PurchaseClothingSetEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null || !session.GetHabbo().InRoom)
                return;

            int setId = packet.PopInt();

            RpClothingStoreManager store = PlusEnvironment.GetRpClothingStoreManager();

            if (!store.TryGetSet(setId, out RpClothingSet set) ||
                !store.IsSetAvailableInRoom(set, session.GetHabbo().CurrentRoomId)) {
                session.SendPacket(new ClothingPurchaseResultComposer(false, "unavailable", setId, 0));
                return;
            }

            if (!set.InStock) {
                session.SendPacket(new ClothingPurchaseResultComposer(false, "out_of_stock", setId, set.Stock));
                return;
            }

            if (set.PartIds.Count == 0) {
                session.SendPacket(new ClothingPurchaseResultComposer(false, "invalid_config", setId, set.Stock));
                return;
            }

            int price = set.EffectivePrice;
            if (session.GetHabbo().Credits < price) {
                session.SendPacket(new ClothingPurchaseResultComposer(false, "not_enough_credits", setId, set.Stock));
                return;
            }

            if (price > 0) {
                session.GetHabbo().Credits -= price;
                session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
            }

            store.ConsumeStock(set);

            session.GetHabbo().GetClothing().AddClothing(set.Name, set.PartIds.ToList());
            session.SendPacket(new FigureSetIdsComposer(session.GetHabbo().GetClothing().GetClothingParts));

            session.SendPacket(new ClothingPurchaseResultComposer(true, "success", setId, set.Stock));

            IReadOnlyList<RpClothingCategory> categories = store.GetCategoriesForRoom(session.GetHabbo().CurrentRoomId);
            session.SendPacket(new ClothingStoreComposer(categories));
        }
    }
}