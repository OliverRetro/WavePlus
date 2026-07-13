using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.TargetLock;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Users
{
    internal class GetSelectedBadgesEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int userId = packet.PopInt();
            Habbo habbo = PlusEnvironment.GetHabboById(userId);
            if (habbo == null)
                return;

            session.SendPacket(new HabboUserBadgesComposer(habbo));

            if (session?.GetHabbo() != null)
            {
                Habbo target = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId)?.GetHabbo();
                if (target != null)
                    TargetLockService.StickyClick(session.GetHabbo(), target);
            }
        }
    }
}