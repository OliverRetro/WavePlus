using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Roleplay.Gang;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Roleplay.Gang
{
    internal class GangDeleteEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            Habbo viewer = session?.GetHabbo();
            if (viewer == null)
                return;

            GroupManager groups = PlusEnvironment.GetGame().GetGroupManager();
            if (!groups.TryGetGangForUser(viewer.Id, out Group gang) || gang == null)
                return;

            if (gang.CreatorId != viewer.Id) {
                session.SendPacket(new RPGangNoticeComposer("Only the owner can disband the gang."));
                return;
            }

            int gangId = gang.Id;
            List<int> memberIds = gang.GetAllMembers;

            groups.DeleteGangPermanent(gangId);

            foreach (int memberId in memberIds)
                GangMottoService.ClearToCitizen(memberId);

            foreach (int memberId in memberIds) {
                Habbo member = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(memberId)?.GetHabbo();
                if (member?.GetStats() == null || member.GetStats().FavouriteGroupId != gangId)
                    continue;

                member.GetStats().FavouriteGroupId = 0;
                member.GetClient()?.SendPacket(new RefreshFavouriteGroupComposer(memberId));
            }

            session.SendPacket(new RPGangNoticeComposer("Gang disbanded."));
            session.SendPacket(new RPGangDataComposer(session));
        }
    }
}