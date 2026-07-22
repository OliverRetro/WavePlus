using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Navigator
{
    public class RemoveFavouriteRoomEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int id = packet.PopInt();

            session.GetHabbo().FavoriteRooms.Remove(id);
            session.SendPacket(new UpdateFavouriteRoomComposer(id, false));

            uint userId = (uint)session.GetHabbo().Id;
            uint roomId = (uint)id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.UserFavorites.Where(f => f.UserId == userId && f.RoomId == roomId).ExecuteDelete();
            }
        }
    }
}