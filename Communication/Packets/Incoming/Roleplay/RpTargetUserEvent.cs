using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.TargetLock;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Roleplay
{
    /// <summary>
    /// Nitro's RPTargetUserComposer (6009). The client sends this on every avatar
    /// selection and sends -1 when the selection is cleared, which makes it the
    /// reliable source for "who is this user pointing at".
    ///
    /// The badge request (3326) used to be the only click signal, but Nitro caches
    /// badges per user, so clicking the same avatar twice - or any avatar whose badges
    /// were already fetched - never reached the server. That left the cached target
    /// pointing at whoever was clicked last, which is why bare :hit could swing at the
    /// wrong player.
    /// </summary>
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