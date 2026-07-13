using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Outgoing.GameCenter
{
    internal class GameAchievementListComposer(GameClient session, ICollection<Achievement> achievements, int gameId) : MessageComposer(ServerPacketHeader.GameAchievementListMessageComposer)
    {
        public Habbo Habbo { get; } = session.GetHabbo();
        public ICollection<Achievement> Achievements { get; } = achievements;
        public int GameId { get; } = gameId;

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(GameId);
            packet.WriteInteger(Achievements.Count);
            foreach (Achievement ach in Achievements.ToList())
            {
                UserAchievement userData = Habbo.GetAchievementData(ach.GroupName);
                int targetLevel = (userData != null ? userData.Level + 1 : 1);

                AchievementLevel targetLevelData = ach.Levels[targetLevel];

                packet.WriteInteger(ach.Id); // ach id
                packet.WriteInteger(targetLevel); // target level
                packet.WriteString(ach.GroupName + targetLevel); // badge
                packet.WriteInteger(targetLevelData.Requirement); // requirement
                packet.WriteInteger(targetLevelData.Requirement); // requirement
                packet.WriteInteger(targetLevelData.RewardAmount); // reward amount
                packet.WriteInteger(0); // ach score
                packet.WriteInteger(userData != null ? userData.Progress : 0); // Current progress
                packet.WriteBoolean(userData != null && (userData.Level >= ach.Levels.Count)); // Set 100% completed(??)
                packet.WriteString(ach.Category);
                packet.WriteString("basejump");
                packet.WriteInteger(0); // total levels
                packet.WriteInteger(0);
            }

            packet.WriteString("");
        }
    }
}