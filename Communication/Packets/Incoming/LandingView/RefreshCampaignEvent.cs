using Plus.Communication.Packets.Outgoing.LandingView;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.LandingView
{
    internal class RefreshCampaignEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            try {
                string parseCampaigns = packet.PopString();
                if (parseCampaigns.Contains("gamesmaker"))
                    return;

                string campaignName = "";
                string[] parser = parseCampaigns.Split(';');

                foreach (var value in parser) {
                    if (string.IsNullOrEmpty(value) || value.EndsWith(","))
                        continue;

                    string[] data = value.Split(',');
                    campaignName = data.Length > 1 ? data[1] : "";
                }

                session.SendPacket(new CampaignComposer(parseCampaigns, campaignName));
            }
            catch {
                //ignored
            }
        }
    }
}