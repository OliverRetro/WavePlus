using System.Collections.Generic;
using Plus.HabboHotel.Achievements;

namespace Plus.Communication.Packets.Outgoing.Inventory.Achievements
{
    internal class BadgeDefinitionsComposer : MessageComposer
    {
        public Dictionary<string, Achievement> Achievements { get; }

        public BadgeDefinitionsComposer(Dictionary<string, Achievement> achievements)
            : base(ServerPacketHeader.BadgeDefinitionsMessageComposer)
        {
            Achievements = achievements;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Achievements.Count);

            foreach (Achievement achievement in Achievements.Values) {
                packet.WriteString(achievement.GroupName.Replace("ACH_", ""));
                packet.WriteInteger(achievement.Levels.Count);
                foreach (AchievementLevel level in achievement.Levels.Values) {
                    packet.WriteInteger(level.Level);
                    packet.WriteInteger(level.Requirement);
                }
            }
        }
    }
}