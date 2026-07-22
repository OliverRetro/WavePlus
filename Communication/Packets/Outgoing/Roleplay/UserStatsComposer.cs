using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Roleplay;

namespace Plus.Communication.Packets.Outgoing.Quests
{
    public class UserStatsComposer : MessageComposer
    {
        public Habbo Habbo { get; }
        public Habbo TargetHabbo { get; }

        public UserStatsComposer(GameClient session, Habbo targetHabbo)
            : base(ServerPacketHeader.UserStatsMessageComposer)
        {
            Habbo = session.GetHabbo();
            TargetHabbo = targetHabbo;
        }

        public override void Compose(ServerPacket packet)
        {
            SerializeUserData(packet, Habbo);

            // Only advertise a target the client can actually read. If the target has no RP
            // stats we must NOT write hasOther=true with no following bytes, or the packet
            // desyncs and the client drops the whole stats update (incl. our own stats).
            bool hasOther = TargetHabbo != null && TargetHabbo.GetRpStats() != null;
            packet.WriteBoolean(hasOther);
            if (hasOther) {
                SerializeUserData(packet, TargetHabbo);
            }
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
            message.WriteInteger(rpStats.Energy);
            message.WriteInteger(UserRpStats.GetMaxEnergy(habbo)); // Max shield, hardcoded for now
            message.WriteInteger(rpStats.Shield);
            message.WriteInteger(100); // Max energy, hardcoded for now
            message.WriteInteger(rpStats.Hunger);
            message.WriteInteger(UserRpStats.GetMaxHunger(habbo));
            message.WriteInteger(habbo.GetAggressionSecondsRemaining());
            message.WriteInteger(habbo.GetAggressionDuration());
        }
    }
}