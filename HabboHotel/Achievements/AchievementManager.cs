using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Plus.Communication.Packets.Outgoing.Inventory.Achievements;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Messenger;

namespace Plus.HabboHotel.Achievements
{
    public class AchievementManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(AchievementManager));

        public Dictionary<string, Achievement> Achievements;

        // Momentary progress buffer: userId -> (achievement group -> accumulated delta).
        // Flushed once a minute (see Game.GameCycle) so bursty events don't spam the client.
        private readonly ConcurrentDictionary<int, ConcurrentDictionary<string, int>> _pendingProgress;

        public AchievementManager()
        {
            Achievements = new Dictionary<string, Achievement>();
            _pendingProgress = new ConcurrentDictionary<int, ConcurrentDictionary<string, int>>();
        }

        public void Init()
        {
            AchievementLevelFactory.GetAchievementLevels(out Achievements);
        }

        public bool ProgressAchievement(GameClient session, string group, int progress, bool fromBeginning = false)
        {
            if (!Achievements.ContainsKey(group) || session == null)
                return false;

            Achievement data = Achievements[group];
            if (data == null) {
                return false;
            }

            UserAchievement userData = session.GetHabbo().GetAchievementData(group);
            if (userData == null) {
                userData = new UserAchievement(group, 0, 0);
                session.GetHabbo().Achievements.TryAdd(group, userData);
            }

            int totalLevels = data.Levels.Count;

            if (userData.Level >= totalLevels)
                return false; // done, no more.

            int workingProgress = fromBeginning ? progress : userData.Progress + progress;
            int newLevel = userData.Level;
            bool leveledUp = false;

            // Carry overflow across as many levels as the gained progress supports, so a large
            // jump (e.g. an old account first satisfying "days registered") lands on the right level.
            while (newLevel < totalLevels) {
                int targetLevel = newLevel + 1;
                AchievementLevel level = data.Levels[targetLevel];

                if (workingProgress < level.Requirement)
                    break;

                workingProgress -= level.Requirement;
                newLevel++;
                leveledUp = true;

                if (targetLevel > 1)
                    session.GetHabbo().GetBadgeComponent().RemoveBadge(Convert.ToString(group + (targetLevel - 1)));
                session.GetHabbo().GetBadgeComponent().GiveBadge(group + targetLevel, true, session);

                session.SendPacket(new AchievementUnlockedComposer(data, targetLevel, level.RewardPoints, level.RewardAmount, level.PointsType));
                session.GetHabbo().GetMessenger().BroadcastAchievement(session.GetHabbo().Id, MessengerEventTypes.AchievementUnlocked, group + targetLevel);

                GrantCurrencyReward(session, level.PointsType, level.RewardAmount);
                session.GetHabbo().GetStats().AchievementPoints += level.RewardPoints;
                session.SendPacket(new AchievementScoreComposer(session.GetHabbo().GetStats().AchievementPoints));
            }

            // No levels left to absorb leftover progress once maxed out.
            if (newLevel >= totalLevels)
                workingProgress = 0;

            userData.Level = newLevel;
            userData.Progress = workingProgress;
            SaveUserAchievement(session.GetHabbo().Id, group, newLevel, workingProgress);

            int displayTarget = newLevel + 1;
            if (displayTarget > totalLevels)
                displayTarget = totalLevels;

            session.SendPacket(new AchievementProgressedComposer(data, displayTarget, data.Levels[displayTarget], totalLevels, userData));
            return leveledUp;
        }

        // Pays out the currency reward in the type configured on the achievement level.
        // pointsType: -1 = credits, 5 = diamonds, anything else (0) = duckets.
        public void QueueProgress(GameClient session, string group, int amount = 1)
        {
            if (session?.GetHabbo() == null || amount <= 0)
                return;

            // Disabled/unknown achievements never enter the dictionary, so silently ignore them.
            if (!Achievements.ContainsKey(group))
                return;

            ConcurrentDictionary<string, int> userPending = _pendingProgress.GetOrAdd(session.GetHabbo().Id, _ => new ConcurrentDictionary<string, int>());
            userPending.AddOrUpdate(group, amount, (_, existing) => existing + amount);
        }

        public void FlushQueuedProgress()
        {
            if (_pendingProgress.IsEmpty)
                return;

            foreach (int userId in _pendingProgress.Keys.ToList()) {
                if (!_pendingProgress.TryRemove(userId, out ConcurrentDictionary<string, int> userPending))
                    continue;

                GameClient session = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
                if (session?.GetHabbo() == null)
                    continue; // user left; buffered progress for this window is dropped

                foreach (KeyValuePair<string, int> entry in userPending) {
                    try {
                        ProgressAchievement(session, entry.Key, entry.Value);
                    } catch (System.Exception e) {
                        Log.Error($"Failed to flush achievement '{entry.Key}' for user {userId}: {e}");
                    }
                }
            }
        }

        private static int GetCumulativeProgress(Achievement data, UserAchievement userData)
        {
            if (userData == null)
                return 0;

            int total = 0;
            for (int level = 1; level <= userData.Level && data.Levels.ContainsKey(level); level++)
                total += data.Levels[level].Requirement;

            return total + userData.Progress;
        }

        public void SetAbsoluteProgress(GameClient session, string group, int absolute)
        {
            if (session?.GetHabbo() == null || absolute <= 0)
                return;

            if (!Achievements.ContainsKey(group))
                return;

            UserAchievement userData = session.GetHabbo().GetAchievementData(group);
            int current = GetCumulativeProgress(Achievements[group], userData);

            int delta = absolute - current;
            if (delta <= 0)
                return;

            ProgressAchievement(session, group, delta);
        }

        public void ResetAchievement(GameClient session, string group)
        {
            if (session?.GetHabbo() == null || !Achievements.ContainsKey(group))
                return;

            UserAchievement userData = session.GetHabbo().GetAchievementData(group);
            if (userData == null || (userData.Level == 0 && userData.Progress == 0))
                return;

            for (int level = 1; level <= userData.Level; level++)
                session.GetHabbo().GetBadgeComponent().RemoveBadge(group + level);

            userData.Level = 0;
            userData.Progress = 0;
            SaveUserAchievement(session.GetHabbo().Id, group, 0, 0);

            // Re-send the (now empty) progress so the client's achievement UI resets.
            Achievement data = Achievements[group];
            if (data.Levels.ContainsKey(1))
                session.SendPacket(new AchievementProgressedComposer(data, 1, data.Levels[1], data.Levels.Count, userData));
        }

        private static void GrantCurrencyReward(GameClient session, int pointsType, int amount)
        {
            if (amount == 0)
                return;

            Users.Habbo habbo = session.GetHabbo();
            switch (pointsType) {
                case -1:
                    habbo.Credits += amount;
                    session.SendPacket(new CreditBalanceComposer(habbo.Credits));
                    break;
                case 5:
                    habbo.Diamonds += amount;
                    session.SendPacket(new HabboActivityPointNotificationComposer(habbo.Diamonds, amount, 5));
                    break;
                default:
                    habbo.Duckets += amount;
                    session.SendPacket(new HabboActivityPointNotificationComposer(habbo.Duckets, amount, 0));
                    break;
            }
        }

        private static void SaveUserAchievement(int userId, string group, int level, int progress)
        {
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            db.UserAchievements.Upsert(new Plus.Database.EF.Entities.UserAchievementEntity
            {
                Userid = (uint)userId,
                Group = group,
                Level = level,
                Progress = progress
            }).Run();
        }

        public ICollection<Achievement> GetGameAchievements(int gameId)
        {
            List<Achievement> achievements = new();

            foreach (Achievement achievement in Achievements.Values.ToList()) {
                if (achievement.Category == "games" && achievement.GameId == gameId)
                    achievements.Add(achievement);
            }

            return achievements;
        }
    }
}