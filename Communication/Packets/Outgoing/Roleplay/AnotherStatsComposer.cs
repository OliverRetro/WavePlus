using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Roleplay;

namespace Plus.Communication.Packets.Outgoing.Quests
{
    public class AnotherStatsComposer : MessageComposer
    {
        public Habbo Habbo { get; }

        public AnotherStatsComposer(GameClient session)
            : base(ServerPacketHeader.AnotherStatsMessageComposer)
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

            var rpStats = habbo.GetRpStats();

            if (rpStats == null)
                return;

            message.WriteInteger(habbo.Id);
            message.WriteString(habbo.Username);
            message.WriteString(habbo.Look);
            message.WriteInteger(rpStats.Health);
            message.WriteInteger(UserRpStats.GetMaxHealth(habbo));
            message.WriteInteger(rpStats.Shield);
            message.WriteInteger(100); // Max shield, hardcoded for now
            message.WriteInteger(rpStats.Energy);
            message.WriteInteger(UserRpStats.GetMaxEnergy(habbo));
        }

    }
}