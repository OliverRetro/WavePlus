using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Crafting
{
    internal class CraftEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            PlusEnvironment.GetCraftingManager().CancelCraft(session?.GetHabbo());
        }
    }
}