using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Rooms.Action;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    internal class UnIgnoreUserEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            Room room = session.GetHabbo().CurrentRoom;
            if (room == null)
                return;

            string username = packet.PopString();

            Habbo player = PlusEnvironment.GetHabboByUsername(username);
            if (player == null)
                return;

            if (!session.GetHabbo().GetIgnores().TryGet(player.Id))
                return;

            if (session.GetHabbo().GetIgnores().TryRemove(player.Id)) {
                uint uid = (uint)session.GetHabbo().Id;
                uint ignoreId = (uint)player.Id;
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    db.UserIgnores.Where(x => x.UserId == uid && x.IgnoreId == ignoreId).ExecuteDelete();
                }

                session.SendPacket(new IgnoreStatusComposer(3, player.Username));
            }
        }
    }
}