using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    internal class GiveRoomScoreEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(session.GetHabbo().CurrentRoomId, out Room room))
                return;

            if (session.GetHabbo().RatedRooms.Contains(room.RoomId) || room.CheckRights(session, true))
                return;

            int rating = packet.PopInt();
            switch (rating) {
                case -1:
                    room.Score--;
                    break;
                case 1:
                    room.Score++;
                    break;
                default:
                    return;
            }

            int roomId = room.RoomId;
            int score = room.Score;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Rooms.Where(r => r.Id == roomId).ExecuteUpdate(s => s.SetProperty(r => r.Score, score));
            }

            session.GetHabbo().RatedRooms.Add(room.RoomId);
            session.SendPacket(new RoomRatingComposer(room.Score, !(session.GetHabbo().RatedRooms.Contains(room.RoomId) || room.CheckRights(session, true))));
        }
    }
}