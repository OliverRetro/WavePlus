using Plus.Communication.Packets.Outgoing.Roleplay.Gang;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Roleplay.Gang
{
    internal class GangInviteEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            Habbo viewer = session?.GetHabbo();
            if (viewer == null)
                return;

            string username = (packet.PopString() ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(username))
                return;

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGangForUser(viewer.Id, out Group gang) || gang == null)
                return;

            bool canInvite = gang.CreatorId == viewer.Id || gang.HasPermission(viewer.Id, GangDefinition.PermGangInvite);
            if (!canInvite) {
                session.SendPacket(new RPGangNoticeComposer("You don't have permission to invite members."));
                return;
            }

            if (gang.MemberCount >= GangDefinition.MemberLimit(gang.Kind)) {
                session.SendPacket(new RPGangNoticeComposer($"Your {GangDefinition.DisplayName(gang.Kind)} is full."));
                return;
            }

            GameClient target = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username);
            if (target?.GetHabbo() == null) {
                session.SendPacket(new RPGangNoticeComposer($"{username} isn't online."));
                return;
            }

            if (gang.IsMember(target.GetHabbo().Id)) {
                session.SendPacket(new RPGangNoticeComposer($"{target.GetHabbo().Username} is already in the gang."));
                return;
            }

            if (GroupInviteService.Invite(session, gang, target))
                session.SendPacket(new RPGangNoticeComposer($"Invite sent to {target.GetHabbo().Username}."));
        }
    }
}