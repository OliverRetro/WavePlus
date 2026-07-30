using System;
using System.Linq;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Level;

namespace Plus.HabboHotel.Roleplay.Utilities
{
    public static class RpAchievementHooks
    {
        public static void OnEnterHotel(GameClient session)
        {
            if (session?.GetHabbo() == null)
                return;

            AchievementManagerSet(session);
        }

        private static void AchievementManagerSet(GameClient session)
        {
            var achievements = PlusEnvironment.GetGame().GetAchievementManager();
            Users.Habbo habbo = session.GetHabbo();

            // ACH_RegistrationDuration — whole days elapsed since the account was created.
            if (habbo.AccountCreated > 0) {
                int daysRegistered = (int)Math.Floor((PlusEnvironment.GetUnixTimestamp() - habbo.AccountCreated) / 86400.0);
                achievements.SetAbsoluteProgress(session, "ACH_RegistrationDuration", daysRegistered);
            }

            // ACH_Profile_Pantheon_ — your current RP level (derived from experience).
            if (habbo.GetRpStats() != null) {
                int level = LevelManager.GetXPInfo(habbo.GetRpStats().Experience).Level;
                achievements.SetAbsoluteProgress(session, "ACH_Profile_Pantheon_", level);
            }

            // ACH_ChatStyleOwner — number of unique chat bubbles owned (the default bubble 0 excluded).
            if (habbo.OwnedChatBubbleIds != null) {
                int uniqueBubbles = habbo.OwnedChatBubbleIds.Where(id => id != 0).Distinct().Count();
                achievements.SetAbsoluteProgress(session, "ACH_ChatStyleOwner", uniqueBubbles);
            }

            // ACH_HappyHour — logged in during the 18:00–19:00 window.
            // NOTE: evaluated against server local time; true per-user local time would require a
            // stored timezone offset, which the client does not currently provide.
            if (DateTime.Now.Hour == 18)
                achievements.QueueProgress(session, "ACH_HappyHour", 1);

            EvaluateLoginStreak(session, achievements, habbo);
        }

        // ACH_Login — a consecutive-day login streak. Each new day continues the streak (+1). The
        // streak's cumulative progress *is* the achievement progress (one point per counted day), so
        // no separate counter is stored. If the user has been offline for more than 48 hours the
        // streak is broken: their progress is wiped and the badge removed, then it restarts at 1.
        private static void EvaluateLoginStreak(GameClient session, Achievements.AchievementManager achievements, Users.Habbo habbo)
        {
            const double breakWindowSeconds = 48 * 3600;

            double now = PlusEnvironment.GetUnixTimestamp();
            double lastOnline = habbo.LastOnline; // previous session's disconnect time (0 = first login ever)

            if (lastOnline <= 0) {
                // First recorded login: begin the streak.
                achievements.ProgressAchievement(session, "ACH_Login", 1);
                return;
            }

            if (now - lastOnline > breakWindowSeconds) {
                // Offline for over 48 hours — streak broken. Wipe it (removes the badge) and restart.
                achievements.ResetAchievement(session, "ACH_Login");
                achievements.ProgressAchievement(session, "ACH_Login", 1);
                return;
            }

            // Within the window: count one point the first time they log in on a new calendar day.
            DateTime lastDate = DateTimeOffset.FromUnixTimeSeconds((long)lastOnline).LocalDateTime.Date;
            if (DateTime.Now.Date > lastDate)
                achievements.ProgressAchievement(session, "ACH_Login", 1);
        }
    }
}