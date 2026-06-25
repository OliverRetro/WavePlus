using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Roleplay
{
    /// <summary>
    /// Requests the latest banking snapshot (ATM + Phone composers) for the requesting user.
    /// No payload required.
    /// </summary>
    internal class RequestBankDataEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session?.GetHabbo() == null)
                return;

            PlusEnvironment.GetBankingManager().SendBankSnapshot(session.GetHabbo());
        }
    }
}
