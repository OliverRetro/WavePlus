using Plus.Communication.Packets.Outgoing.Roleplay.Gang;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Roleplay.Gang
{
    internal class GangInviteRespondEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            Habbo viewer = session?.GetHabbo();
            if (viewer == null)
                return;

            int inviteId = packet.PopInt();
            bool accepted = packet.PopBoolean();

            GroupInviteService.Respond(session, inviteId, accepted);

            if (accepted && PlusEnvironment.GetGame().GetGroupManager().TryGetGangForUser(viewer.Id, out Group gang) && gang != null) {
                GangMottoService.ApplyGangMotto(viewer.Id, gang);

                session.SendPacket(new RPGangNoticeComposer(2, $"You are now a member of {gang.Name}.",
                    "Gang Joined", $"You are now a member of {gang.Name}.", gang.Colour1, gang.Colour2));
            }

            session.SendPacket(new RPGangDataComposer(session));
        }
    }
}