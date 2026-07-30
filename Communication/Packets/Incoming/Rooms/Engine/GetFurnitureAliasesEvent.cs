using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    internal class GetFurnitureAliasesEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            // FurnitureAliases is now pushed proactively by EnterRoom() on every room entry
        }
    }
}