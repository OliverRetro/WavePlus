using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Roleplay
{
    /// <summary>
    /// Requests the latest banking snapshot for the requesting user.
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