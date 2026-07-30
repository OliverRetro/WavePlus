using System;
using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Roleplay;

namespace Plus.Communication.Packets.Incoming.Roleplay
{
    internal class SpendAttributePointEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            Habbo habbo = session?.GetHabbo();
            if (habbo == null)
                return;

            int attribute = packet.PopInt();

            UserRpStats stats = habbo.GetRpStats();
            if (stats == null)
                return;

            // No points left in the pool -> nothing to do (the client shouldn't have shown a "+").
            if (stats.GetAvailablePoints() <= 0)
                return;

            switch (attribute) {
                case 0:
                    stats.Strength++;
                    break;
                case 1:
                    stats.Knowledge++;
                    break;
                default:
                    return; // unknown attribute id
            }

            stats.AttributePoints = Math.Max(0, stats.AttributePoints - 1);
            habbo.SaveRpStats();

            ResendOwnProfile(session, habbo);
        }

        private static void ResendOwnProfile(GameClient session, Habbo habbo)
        {
            List<Group> groups = PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(habbo.Id);

            uint uid = (uint)habbo.Id;
            int friendCount;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                friendCount = db.MessengerFriendships.Count(f => f.UserOneId == uid || f.UserTwoId == uid);
            }

            session.SendPacket(new ProfileInformationComposer(habbo, session, groups, friendCount));
        }
    }
}