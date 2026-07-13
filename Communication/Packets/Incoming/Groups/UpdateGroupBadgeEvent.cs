using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Incoming.Groups
{
    internal class UpdateGroupBadgeEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int groupId = packet.PopInt();

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out Group group))
                return;

            if (session.GetHabbo() == null)
                return;

            if (!session.GetHabbo().GetPermissions().HasRight("group_management_override") && group.CreatorId != session.GetHabbo().Id)
                return;

            int count = packet.PopInt();

            string badge = "";

            int baseValue = 1;
            while (baseValue < count)
            {
                int partId = packet.PopInt();
                int colour = packet.PopInt();
                int position = packet.PopInt();

                badge += BadgePartUtility.WorkBadgeParts(baseValue == 1, partId.ToString(), colour.ToString(), position.ToString());
                baseValue += 3;
            }

            group.Badge = (string.IsNullOrWhiteSpace(badge) ? "b05114s06114" : badge);
            group.MarkDirty(); // cached; written to DB on the 15-minute group flush

            session.SendPacket(new GroupInfoComposer(group, session));
        }
    }
}
