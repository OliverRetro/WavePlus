using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using log4net;
using Plus.Communication.Packets.Outgoing.Rooms.Session;

namespace Plus.Communication.Packets.Incoming.Rooms.Engine
{
    internal class GetFurnitureAliasesEvent : IPacketEvent
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GetFurnitureAliasesEvent));

        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!PlusEnvironment.GetGame().GetRoomManager().TryLoadRoom(session.GetHabbo().CurrentRoomId, out Room room))
            {
                Log.Warn("Couldn't load room for GetFurnitureAliasesEvent: " + session.GetHabbo().CurrentRoomId);
                session.SendPacket(new CloseConnectionComposer());
                return;
            }

            if (room == null || room.GetGameMap() == null)
            {
                Log.Debug("Room or GameMap null");
                return;
            }

            session.SendPacket(new HeightMapComposer(room));
            session.SendPacket(new FloorHeightMapComposer(room.GetGameMap().Model.GetRelativeHeightmap(), room.GetGameMap().StaticModel.WallHeight));

            //session.GetHabbo().PrepareRoom(room.Id, room.Password);

            session.SendPacket(new FurnitureAliasesComposer());
        }
    }
}