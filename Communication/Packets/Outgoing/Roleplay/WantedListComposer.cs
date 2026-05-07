using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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
            packet.WriteInteger(2); // size
            // loop starts here
            packet.WriteString("Oliver"); // username
            packet.WriteString("hd-180-7.sh-290-1408.lg-275-79.ch-878-66-1408.hr-175-40.wa-2007-0.ha-6246-0"); // user look
            packet.WriteInteger(5); // stars
            packet.WriteString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")); // time, can also be "" for nothing
            // ends here
            packet.WriteString("Sarbaz"); // username
            packet.WriteString("hd-180-7.sh-290-1408.lg-275-79.ch-878-66-1408.hr-175-40.wa-2007-0.ha-6246-0"); // user look
            packet.WriteInteger(3); // stars
            packet.WriteString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")); // time, can also be "" for nothing
        }
    }
}
