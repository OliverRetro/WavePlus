using System.Collections.Generic;
using Plus.HabboHotel.Roleplay.Crime;

namespace Plus.Communication.Packets.Outgoing.Roleplay
{
    internal class WantedListComposer : MessageComposer
    {
        public WantedListComposer()
            : base(ServerPacketHeader.WantedListMessageComposer)
        {
        }

        public override void Compose(ServerPacket packet)
        {
            // Cached, most-wanted-first (highest visible stars).
            List<RpCrimeManager.WantedRow> rows = PlusEnvironment.GetRpCrimeManager().GetWantedList();

            packet.WriteInteger(rows.Count);
            foreach (RpCrimeManager.WantedRow row in rows) {
                packet.WriteString(row.Username);
                packet.WriteString(row.Look);
                packet.WriteInteger(row.Stars);
                packet.WriteString(row.Time.ToString("yyyy-MM-dd HH:mm:ss"));
            }
        }
    }
}