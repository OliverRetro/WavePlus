using Plus.HabboHotel.Items.Data.Moodlight;

namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.Moodlight
{
    internal class MoodlightConfigComposer : MessageComposer
    {
        public MoodlightData Data { get; }

        public MoodlightConfigComposer(MoodlightData moodlightData)
            : base(ServerPacketHeader.MoodlightConfigMessageComposer)
        {
            Data = moodlightData;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Data.Presets.Count);
            packet.WriteInteger(Data.CurrentPreset);

            int i = 1;
            foreach (MoodlightPreset preset in Data.Presets) {
                packet.WriteInteger(i);
                packet.WriteInteger(preset.BackgroundOnly ? 2 : 1);
                packet.WriteString(preset.ColorCode);
                packet.WriteInteger(preset.ColorIntensity);
                i++;
            }
        }
    }
}