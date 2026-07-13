using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Outgoing.Roleplay
{
    public class OfferItemComposer(GameClient session, Habbo targetHabbo) : MessageComposer(ServerPacketHeader.OfferItemMessageComposer)
    {
        public Habbo Habbo { get; } = session.GetHabbo();
        public Habbo TargetHabbo { get; } = targetHabbo;

        public override void Compose(ServerPacket packet)
        {
            SerializeUserData(packet, Habbo);
        }

        public static void SerializeUserData(ServerPacket message, Habbo habbo)
        {
            if (habbo == null)
                return;

            message.WriteString(habbo.Id.ToString()); // offer id
            message.WriteString(habbo.Username); // item name
            message.WriteInteger(0); // offer price, hardcoded for now
            message.WriteString(habbo.Username);
            message.WriteString(habbo.Look);
        }
    }
}