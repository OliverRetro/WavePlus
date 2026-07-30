using System.Collections.Generic;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Crafting
{
    internal class CraftSecretEvent : IPacketEvent
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

            PlusEnvironment.GetCraftingManager().StartCraft(session, objectId, guess);
        }
    }
}