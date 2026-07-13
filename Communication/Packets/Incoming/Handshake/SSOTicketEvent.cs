using log4net;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Handshake
{
    public class SSOTicketEvent : IPacketEvent
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(SSOTicketEvent));

        public void Parse(GameClient session, ClientPacket packet)
        {
            if (session == null || session.GetHabbo() != null)
                return;

            string sso = packet.PopString();

            if (string.IsNullOrEmpty(sso) || sso.Length < 15)
            {
                Log.Debug("Invalid SSO Ticket, disconnecting client");
                session.Disconnect();
                return;
            }

            session.TryAuthenticate(sso);
        }
    }
}