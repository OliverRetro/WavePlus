using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Macros;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Outgoing.Overlay
{
    /// <summary>
    /// Helper for pushing JSON messages to the gaz-js web overlay (via the habblet bridge).
    /// Each method mirrors a header the gaz-js CommunicationManager listens for.
    /// </summary>
    public static class WebOverlay
    {
        public static void Send(GameClient session, string header, object data)
        {
            session?.SendPacket(new WebOverlayComposer(header, data));
        }

        public static void SendSessionData(GameClient session)
        {
            Habbo habbo = session?.GetHabbo();
            if (habbo == null) return;

            Send(session, "session_data", new
            {
                id = habbo.Id,
                username = habbo.Username,
                look = habbo.Look,
                credits = habbo.Credits
            });
        }

        public static void SendCredits(GameClient session)
        {
            Habbo habbo = session?.GetHabbo();
            if (habbo == null) return;

            Send(session, "update_credits", new { credits = habbo.Credits });
        }

        public static void SendHealth(GameClient session)
        {
            Habbo habbo = session?.GetHabbo();
            UserRpStats stats = habbo?.GetRpStats();
            if (stats == null) return;

            Send(session, "health_data", new
            {
                health = stats.Health,
                maxhealth = UserRpStats.GetMaxHealth(habbo)
            });
        }

        public static void SendShield(GameClient session)
        {
            Habbo habbo = session?.GetHabbo();
            UserRpStats stats = habbo?.GetRpStats();
            if (stats == null) return;

            Send(session, "shield_data", new
            {
                shield = stats.Shield,
                maxshield = 100
            });
        }

        public static void SendOnline(GameClient session, bool online)
        {
            Send(session, "online", new { online });
        }

        public static void SendMacroSet(GameClient session, Macro macro, bool isEnabled)
        {
            if (macro == null)
            {
                Send(session, "macro_set", new { isEnabled });
                return;
            }

            Send(session, "macro_set", new
            {
                isEnabled,
                id = macro.Id,
                name = macro.Name,
                sets = macro.Sets
            });
        }

        public static void SendMacroList(GameClient session, IEnumerable<Macro> macros)
        {
            Send(session, "macro_list", new { sets = macros.Select(m => new { id = m.Id, name = m.Name }).ToArray() });
        }

        public static void SendSettings(GameClient session)
        {
            Habbo habbo = session?.GetHabbo();
            Plus.HabboHotel.Roleplay.Settings.UserRpSettings settings = habbo?.GetRpSettings();
            if (settings == null) return;

            Send(session, "settings_data", new
            {
                clickThrough = settings.ClickThrough,
                autoSalary = settings.AutoSalaryDeposit,
                furnitureTrading = settings.FurnitureTrading,
                autoEvent = settings.AutoEventTransfer,
                voiceChat = settings.VoiceChat,
                livefeed = settings.Livefeed,
                alerts = settings.Alerts,
                // The client greys out / blocks "auto salary" unless the user owns a bank account.
                hasBankAccount = habbo.GetBankAccount() != null
            });
        }
    }
}
