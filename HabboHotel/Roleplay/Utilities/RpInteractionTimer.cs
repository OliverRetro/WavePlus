using Plus.Communication.Packets.Outgoing.Overlay;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;
using System;

namespace Plus.HabboHotel.Roleplay.Utilities
{
    public static class RpInteractionTimer
    {
        public const int BaseSeconds = 20;

        public const int MinimumSeconds = 2;

        public const double CompletionGraceSeconds = 1.0;

        public static int GetSeconds(Habbo habbo)
        {
            int knowledge = Math.Max(0, habbo?.GetRpStats()?.Knowledge ?? 0);

            return Math.Max(MinimumSeconds, BaseSeconds - Users.Roleplay.UserRpStats.GetAttributeBonus(knowledge));
        }

        public static int GetSeconds(GameClient session) => GetSeconds(session?.GetHabbo());

        public static int Begin(GameClient session, string label)
        {
            int seconds = GetSeconds(session);

            WebOverlay.SendInteractionTimer(session, label, seconds);

            return seconds;
        }

        public static void Cancel(GameClient session)
        {
            if (session != null)
                WebOverlay.CancelInteractionTimer(session);
        }

        public static void Cancel(int userId) => Cancel(PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId));
    }
}