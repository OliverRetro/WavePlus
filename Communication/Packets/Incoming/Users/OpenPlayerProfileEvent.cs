using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Users
{
    internal class OpenPlayerProfileEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int userId = packet.PopInt();
            packet.PopBoolean(); //IsMe?

            Habbo targetData = PlusEnvironment.GetHabboById(userId);
            if (targetData == null) {
                session.SendNotification("An error occurred while finding that user's profile.");
                return;
            }

            List<Group> groups = PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(targetData.Id);

            uint uid = (uint)userId;
            int friendCount;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                friendCount = db.MessengerFriendships.Count(f => f.UserOneId == uid || f.UserTwoId == uid);
            }

            session.SendPacket(new ProfileInformationComposer(targetData, session, groups, friendCount));
        }
    }
}