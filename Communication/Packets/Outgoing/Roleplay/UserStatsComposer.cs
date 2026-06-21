using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Outgoing.Quests
{
    public class UserStatsComposer : MessageComposer
    {
        public Habbo Habbo { get; }
        public Habbo targetHabbo { get; }

        public UserStatsComposer(GameClient session, Habbo targetHabbo)
            : base(ServerPacketHeader.UserStatsMessageComposer)
        {
            Habbo = session.GetHabbo();
            this.targetHabbo = targetHabbo;
        }

        public override void Compose(ServerPacket packet)
        {
            SerializeUserData(packet, Habbo);
            bool hasOther = targetHabbo != null;
            packet.WriteBoolean(hasOther);
            if (hasOther)
            {
                SerializeUserData(packet, targetHabbo);
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
            message.WriteInteger(rpStats.Shield);
            message.WriteInteger(100); // Max shield, hardcoded for now
            message.WriteInteger(rpStats.Energy);
            message.WriteInteger(100); // Max energy, hardcoded for now
        }
    }
}