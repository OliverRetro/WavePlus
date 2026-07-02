using Plus.Communication.Packets.Outgoing.Roleplay;
using Plus.Communication.Packets.Outgoing.Users;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.TargetLock;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Roleplay
{
    internal class RequestWearingBadgesEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
                return;

            int userId = packet.PopInt();
            Habbo target = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId)?.GetHabbo()
                           ?? PlusEnvironment.GetHabboById(userId);
            if (target == null)
                return;

            session.SendPacket(new HabboUserBadgesComposer(target));

            Habbo requester = session.GetHabbo();
            if (!TargetLockService.IsTargetLocked(requester, target)) {
                TargetLockService.SetClickedUser(requester, target.Username);
                session.SendPacket(new TargetLockComposer(target.Username, false));
            }

            // Refresh the requester's own RP stats (life/energy/shield).
            requester.TrySendUserStatsUpdate(true);
        }
    }
}
