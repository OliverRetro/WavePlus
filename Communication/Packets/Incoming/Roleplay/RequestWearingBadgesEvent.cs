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

            // A click never locks. Unlocked -> swap freely; locked -> keep the locked target.
            TargetLockService.HandleAvatarClick(session.GetHabbo(), target);
        }
    }
}