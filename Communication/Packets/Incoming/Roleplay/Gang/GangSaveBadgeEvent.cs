using Plus.Communication.Packets.Outgoing.Roleplay.Gang;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Roleplay.Gang
{
    internal class GangSaveBadgeEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            Habbo viewer = session?.GetHabbo();
            if (viewer == null)
                return;

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGangForUser(viewer.Id, out Group gang) || gang == null)
                return;

            bool canEdit = gang.CreatorId == viewer.Id || gang.HasPermission(viewer.Id, GangDefinition.PermEditGroup);
            if (!canEdit) {
                session.SendPacket(new RPGangNoticeComposer("You don't have permission to edit the gang badge."));
                return;
            }

            // Wire shape mirrors the normal group badge editor: a length-prefixed flat
            // list of [key, colour, position] triplets. The first triplet is the base.
            int count = packet.PopInt();
            string badge = "";
            int baseValue = 1;
            while (baseValue < count) {
                int partId = packet.PopInt();
                int colour = packet.PopInt();
                int position = packet.PopInt();

                badge += BadgePartUtility.WorkBadgeParts(baseValue == 1, partId.ToString(), colour.ToString(), position.ToString());
                baseValue += 3;
            }

            gang.Badge = string.IsNullOrWhiteSpace(badge) ? GangDefinition.DefaultBadge : badge;
            gang.MarkDirty();

            session.SendPacket(new RPGangNoticeComposer("Gang badge saved."));
            session.SendPacket(new RPGangDataComposer(session));
        }
    }
}