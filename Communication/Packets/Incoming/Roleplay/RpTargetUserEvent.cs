using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.TargetLock;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Roleplay
{
    internal class RpTargetUserEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
                return;

            int userId = packet.PopInt();

            // -1 (or any non-positive id) means the client dropped its selection.
            // Only clear an unlocked selection: a lock is released by :unlocktarget,
            // a reconnect, or picking a different target - never by a stray deselect.
            if (userId <= 0) {
                TargetLockService.ClearClickedTarget(session.GetHabbo());
                return;
            }

            Habbo target = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId)?.GetHabbo();
            if (target == null)
                return;

            TargetLockService.HandleAvatarClick(session.GetHabbo(), target);
        }
    }
}