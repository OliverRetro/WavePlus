using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Settings;

namespace Plus.Communication.Packets.Outgoing.Roleplay
{
    internal class ClientVisualSettingsComposer : MessageComposer
    {
        private readonly bool _chatHigher;
        private readonly int _fpsMax;
        private readonly bool _dragRooms;

        public ClientVisualSettingsComposer(GameClient session)
            : base(ServerPacketHeader.ClientVisualSettingsMessageComposer)
        {
            UserRpSettings settings = session?.GetHabbo()?.GetRpSettings();
            _chatHigher = settings?.ChatHigher ?? false;
            _fpsMax = UserRpSettings.ClampFps(settings?.FpsMax ?? UserRpSettings.FpsMaxDefault);
            _dragRooms = settings?.DragRooms ?? true;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteBoolean(_chatHigher);
            packet.WriteInteger(_fpsMax);
            packet.WriteBoolean(_dragRooms);
        }
    }
}