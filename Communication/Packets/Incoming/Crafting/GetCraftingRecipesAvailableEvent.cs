using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Crafting;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Crafting;

namespace Plus.Communication.Packets.Incoming.Crafting
{
    internal class GetCraftingRecipesAvailableEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int objectId = packet.PopInt();
            int count = packet.PopInt();

            Dictionary<(string Type, int Id), int> guess = new();
            for (int i = 0; i < count; i++) {
                int typeFlag = packet.PopInt();
                int itemId = packet.PopInt();
                string type = typeFlag == 1 ? "rp_weapon" : "rp_item";

                guess.TryGetValue((type, itemId), out int have);
                guess[(type, itemId)] = have + 1;
            }

            ICraftingSource source = CraftingContext.Resolve(session, objectId);
            if (source == null)
                return;

            (int state, int hintCount, CraftingRecipe exact) = PlusEnvironment.GetCraftingManager().EvaluateHint(guess, source.IsUnlocked);

            if (state == 2 && exact != null) {
                CraftingManager.Describe(exact.RewardType, exact.RewardId, out string name, out string icon, out _);
                session.SendPacket(new CraftingFoundComposer(2, 0, exact.Name, $"{exact.RewardType}:{exact.RewardId}", name, icon));
                return;
            }

            session.SendPacket(new CraftingFoundComposer(state, hintCount));
        }
    }
}