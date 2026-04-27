using Plus.HabboHotel.GameClients;
using log4net;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    internal class GetFurnitureAliasesEvent : IPacketEvent
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GetFurnitureAliasesEvent));

        public void Parse(GameClient session, ClientPacket packet)
        {
            // FurnitureAliases is now pushed proactively by EnterRoom() on every room entry,
            // so we must not respond here — doing so would cause a duplicate FurnitureAliases
            // packet, triggering two GetRoomEntryData requests and triple-adding the user.
        }
    }
}
