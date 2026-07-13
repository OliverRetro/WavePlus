using Plus.Communication.Packets.Outgoing.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.Communication.Packets.Incoming.Groups
{
    internal class UpdateGroupIdentityEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int groupId = packet.PopInt();
            string name = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(packet.PopString());
            string desc = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(packet.PopString());

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out Group group))
                return;

            if (group.CreatorId != session.GetHabbo().Id)
                return;

            group.Name = name;
            group.Description = desc;
            group.MarkDirty(); // cached; written to DB on the 15-minute group flush

            session.SendPacket(new GroupInfoComposer(group, session));
        }
    }
}