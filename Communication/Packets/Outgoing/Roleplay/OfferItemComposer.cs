using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Outgoing.Quests
{
    public class OfferItemComposer : MessageComposer
    {
        public Habbo Habbo { get; }

        public OfferItemComposer(GameClient session, Habbo targetHabbo)
            : base(ServerPacketHeader.OfferItemMessageComposer)
        {
            Habbo = session.GetHabbo();
        }

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