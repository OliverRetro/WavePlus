using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Camera;
using Plus.Database.EF;
using Plus.HabboHotel.Camera;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Camera
{
    public class RequestMyPhotosEvent : IPacketEvent
    {
        private const int MaxPhotos = 50;

        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
                return;

            int userId = session.GetHabbo().Id;

            List<GalleryPhoto> photos;

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                photos = db.CameraPhotos
                    .Where(p => p.UserId == userId)
                    .GroupBy(p => p.Url)
                    .Select(g => new
                    {
                        Url = g.Key,
                        Timestamp = g.Max(x => x.Timestamp),
                        RoomId = g.Max(x => x.RoomId),
                        LatestId = g.Max(x => x.Id)
                    })
                    .OrderByDescending(x => x.Timestamp)
                    .ThenByDescending(x => x.LatestId)
                    .Take(MaxPhotos)
                    .ToList()
                    .Select(x => new GalleryPhoto(x.Url, x.Timestamp, x.RoomId))
                    .ToList();
            }

            session.SendPacket(new MyPhotosComposer(photos));
        }
    }
}