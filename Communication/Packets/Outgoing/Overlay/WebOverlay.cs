using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Macros;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Roleplay.Skill;
using Plus.HabboHotel.Rooms.Chat.Styles;
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

        public static void SendInteractionTimer(GameClient session, string label, int seconds)
        {
            if (session == null || seconds <= 0)
                return;

            Send(session, "interaction_timer", new
            {
                label = label ?? "",
                seconds
            });
        }

        public static void CancelInteractionTimer(GameClient session)
        {
            Send(session, "interaction_timer_cancel", new { });
        }

        private const string DefaultGroupBadgeUrl = "https://waverp.online/habbo-imaging/badge/%badge%.png";

        public static string GetGroupBadgeUrl(string badge)
        {
            if (string.IsNullOrEmpty(badge))
                return "";

            string template = PlusEnvironment.GetConfig().Data.TryGetValue("group.badge.url", out string configured) && !string.IsNullOrWhiteSpace(configured)
                ? configured
                : DefaultGroupBadgeUrl;

            return template.Replace("%badge%", badge);
        }

        public static void SendGroupInvite(GameClient session, int inviteId, string sender, string groupName, int groupId, string badge)
        {
            Send(session, "group_invite", new
            {
                inviteId,
                sender = sender ?? "",
                groupName = groupName ?? "",
                groupId,
                badge = badge ?? "",
                badgeUrl = GetGroupBadgeUrl(badge)
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

        public static void SendMyItems(GameClient session)
        {
            Habbo habbo = session?.GetHabbo();
            if (habbo == null) return;

            Send(session, "my_items", new
            {
                username = habbo.Username,
                look = habbo.Look,
                maxEquippedSkills = UserRpSkills.MaxEquippedSkills,
                equippedSkillCount = habbo.GetRpSkills()?.EquippedCount() ?? 0,
                skills = BuildSkills(habbo),
                bubbles = BuildBubbles(habbo),
                skins = BuildSkins(habbo)
            });
        }

        // Every skill is listed whether or not the user has unlocked it — an unlocked-less skill
        // just reports tier 0 and canEquip=false, which the client renders greyed out.
        private static object[] BuildSkills(Habbo habbo)
        {
            UserRpSkills userSkills = habbo.GetRpSkills();
            if (userSkills == null)
                return new object[0];

            return PlusEnvironment.GetSkillManager().GetSkills()
                .OrderBy(skill => skill.Id)
                .Select(skill =>
                {
                    int tier = userSkills.GetLevel(skill.Id);
                    int progress = userSkills.GetProgress(skill.Id);
                    bool unlocked = tier > 0;
                    bool equipped = userSkills.IsEquipped(skill.Id);

                    // Progress toward the next tier, so the client can draw a bar. A maxed skill
                    // reports its final threshold for both, leaving the bar full.
                    SkillLevel next = skill.Levels.Values.OrderBy(x => x.Level)
                        .FirstOrDefault(x => x.RequiredProgress > progress);
                    int currentThreshold = skill.Levels.Values.Where(x => x.Level == tier)
                        .Select(x => x.RequiredProgress).FirstOrDefault();

                    return (object)new
                    {
                        id = skill.Id,
                        name = skill.Name,
                        description = skill.Description,
                        badgeCode = skill.BadgeCode ?? "",
                        tier,
                        maxTier = skill.Levels.Count == 0 ? 0 : skill.Levels.Values.Max(x => x.Level),
                        progress,
                        currentThreshold,
                        nextThreshold = next?.RequiredProgress ?? currentThreshold,
                        unlocked,
                        equipped,
                        // Equipping is blocked while locked, or once the 4-skill limit is reached.
                        canEquip = unlocked && (equipped || userSkills.CanEquipMoreSkills())
                    };
                })
                .ToArray();
        }

        private static object[] BuildBubbles(Habbo habbo)
        {
            ChatStyleManager styles = PlusEnvironment.GetGame().GetChatManager().GetChatStyles();
            List<object> bubbles = new();

            foreach (int bubbleId in habbo.GetOwnedChatBubbleIds().Distinct().OrderBy(x => x))
            {
                if (!styles.TryGetStyle(bubbleId, out ChatStyle style))
                    continue;

                // Rank-gated bubbles disappear from the list if the user loses the right.
                if (!string.IsNullOrEmpty(style.RequiredRight) && !habbo.GetPermissions().HasRight(style.RequiredRight))
                    continue;

                bubbles.Add(new
                {
                    id = style.Id,
                    name = string.IsNullOrWhiteSpace(style.Name) ? "Bubble " + style.Id : style.Name,
                    url = style.Url ?? "",
                    fontColor = string.IsNullOrWhiteSpace(style.FontColor) ? "#000000" : style.FontColor,
                    equipped = habbo.CustomBubbleId == style.Id
                });
            }

            return bubbles.ToArray();
        }

        // The user's owned weapon skins. Only one skin per weapon can be equipped at a time.
        private static object[] BuildSkins(Habbo habbo)
        {
            UserRpWeaponSkins userSkins = habbo.GetRpWeaponSkins();
            if (userSkins == null)
                return new object[0];

            return userSkins.Skins
                .Where(userSkin => userSkin.SkinData != null)
                .OrderBy(userSkin => userSkin.SkinData.WeaponId)
                .ThenBy(userSkin => userSkin.SkinData.Name)
                .Select(userSkin =>
                {
                    Weapon weapon = PlusEnvironment.GetWeaponManager().GetWeaponById(userSkin.SkinData.WeaponId);
                    return (object)new
                    {
                        id = userSkin.Id,
                        skinId = userSkin.SkinId,
                        weaponId = userSkin.SkinData.WeaponId,
                        name = userSkin.SkinData.Name,
                        weaponName = weapon?.Name ?? "",
                        image = weapon?.Image ?? "",
                        rarity = userSkin.SkinData.Rarity,
                        equipped = userSkin.Equipped
                    };
                })
                .ToArray();
        }
    }
}
