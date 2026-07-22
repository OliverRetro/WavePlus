using Plus.Communication.Packets.Outgoing.Rooms.Action;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.GameClients;
// Disambiguates the bare name from the scaffolded EF entity Plus.Database.EF.Entities.Room.
using Room = Plus.HabboHotel.Rooms.Room;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Rooms.Action
{
    internal class IgnoreUserEvent : IPacketEvent
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
            if (player == null || player.GetPermissions().HasRight("mod_tool"))
                return;

            if (session.GetHabbo().GetIgnores().TryGet(player.Id))
                return;

            if (session.GetHabbo().GetIgnores().TryAdd(player.Id)) {
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    db.UserIgnores.Add(new UserIgnoreEntity { UserId = (uint)session.GetHabbo().Id, IgnoreId = (uint)player.Id });
                    db.SaveChanges();
                }

                session.SendPacket(new IgnoreStatusComposer(1, player.Username));

                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(session, "ACH_SelfModIgnoreSeen", 1);
            }
        }
    }
}