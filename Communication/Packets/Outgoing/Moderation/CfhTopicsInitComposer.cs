using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Moderation;

namespace Plus.Communication.Packets.Outgoing.Moderation
{
    internal class CfhTopicsInitComposer : MessageComposer
    {
        public Dictionary<string, List<ModerationPresetActions>> UserActionPresets { get; }

        public CfhTopicsInitComposer(Dictionary<string, List<ModerationPresetActions>> userActionPresets)
            : base(ServerPacketHeader.CfhTopicsInitMessageComposer)
        {
            UserActionPresets = userActionPresets;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(UserActionPresets.Count);
            foreach (KeyValuePair<string, List<ModerationPresetActions>> cat in UserActionPresets.ToList()) {
                packet.WriteString(cat.Key);
                packet.WriteInteger(cat.Value.Count);
                foreach (ModerationPresetActions preset in cat.Value.ToList()) {
                    packet.WriteString(preset.Caption);
                    packet.WriteInteger(preset.Id);
                    packet.WriteString(preset.Type);
                }
            }
        }
    }
}