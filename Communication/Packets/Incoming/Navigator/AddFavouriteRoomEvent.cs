using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Navigator
{
    public class AddFavouriteRoomEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null)
                return;

            int roomId = packet.PopInt();

            if (!RoomFactory.TryGetData(roomId, out RoomData data))
                return;

            if (data == null || session.GetHabbo().FavoriteRooms.Count >= 30 || session.GetHabbo().FavoriteRooms.Contains(roomId)) {
                // send packet that favourites is full.
                return;
            }

            session.GetHabbo().FavoriteRooms.Add(roomId);
            session.SendPacket(new UpdateFavouriteRoomComposer(roomId, true));

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.UserFavorites.Add(new UserFavoriteEntity
                {
                    UserId = (uint)session.GetHabbo().Id,
                    RoomId = (uint)roomId
                });
                db.SaveChanges();
            }
        }
    }
}