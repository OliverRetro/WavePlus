using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Plus.Communication.Packets.Outgoing.Overlay;
using Plus.Communication.Packets.Outgoing.Roleplay;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Macros;
using Plus.HabboHotel.Roleplay.RpItem;
using Plus.HabboHotel.Roleplay.RpItem.Item;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Roleplay.Settings;
using Plus.HabboHotel.Roleplay.Skill;
using Plus.HabboHotel.Roleplay.Stock;
using Plus.HabboHotel.Roleplay.TargetLock;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Roleplay;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming
{
    internal class WebOverlayCallbackEvent : IPacketEvent
    {
        // Macro placeholder that stands in for the user's current RP target (locked or clicked).
        // e.g. a bind of ":hit x" fires against whoever is targeted, not a player named "x".
        private const string MacroTargetPlaceholder = "x";

        public void Parse(GameClient session, ClientPacket packet)
        {
            // DIAGNOSTIC: proves packet 314 reached this handler at all.
            string payload = packet.PopString();
            if (IsDebugEnabled()) Console.WriteLine("[WebOverlay] <-- packet 314 received, raw payload: " + payload);

            if (session?.GetHabbo() == null) {
                if (IsDebugEnabled()) Console.WriteLine("[WebOverlay] aborted: session/habbo is null");
                return;
            }

            payload = payload.Replace("&#47;", "/");
            if (string.IsNullOrWhiteSpace(payload)) {
                if (IsDebugEnabled()) Console.WriteLine("[WebOverlay] aborted: empty payload");
                return;
            }

            try {
                using JsonDocument doc = JsonDocument.Parse(payload);
                JsonElement root = doc.RootElement;

                if (!root.TryGetProperty("header", out JsonElement headerEl)) {
                    if (IsDebugEnabled()) Console.WriteLine("[WebOverlay] aborted: no 'header' property in payload");
                    return;
                }

                string header = headerEl.GetString() ?? string.Empty;
                JsonElement data = root.TryGetProperty("data", out JsonElement d) ? d : default;

                if (IsDebugEnabled()) Console.WriteLine("[WebOverlay] dispatching header: '" + header + "'");
                Dispatch(header, data, session);
            } catch (Exception ex) {
                if (IsDebugEnabled()) Console.WriteLine("[WebOverlay] malformed message: " + ex.Message);
            }
        }

        private static bool IsDebugEnabled()
        {
            return PlusEnvironment.GetSettingsManager()?.TryGetValue("rp.weboverlay.debug") == "1";
        }

        private static void Dispatch(string header, JsonElement data, GameClient session)
        {
            switch (header) {
                // The overlay finished loading / handshake — push everything it needs.
                case "sso":
                case "js_loaded":
                case "loaded":
                    WebOverlay.SendSessionData(session);
                    WebOverlay.SendHealth(session);
                    WebOverlay.SendShield(session);
                    WebOverlay.SendOnline(session, true);
                    WebOverlay.SendSettings(session);
                    PlusEnvironment.GetGame().GetMacroManager().EnsureDefaultSets(session.GetHabbo().Id);
                    SendMacroList(session);
                    SendMacroSet(session);
                    break;

                case "request_settings":
                    WebOverlay.SendSettings(session);
                    break;

                case "set_setting":
                    SetSetting(data, session);
                    break;

                case "request_credits":
                    WebOverlay.SendCredits(session);
                    break;

                case "pong":
                    // keep-alive, nothing to do
                    break;

                case "open_inventory":
                    OpenInventory(session);
                    break;

                // Double-click in the overlay — use/activate an owned item.
                case "use_item":
                    UseItem(data, session);
                    break;

                // Drag & drop reorder — persist the new grid positions.
                case "save_inventory":
                    SaveInventory(data, session);
                    break;

                // Drag a weapon onto the primary equipped slot.
                case "equip_weapon":
                    EquipWeapon(data, session);
                    break;

                // Drag/double-click the active weapon out of the primary slot.
                case "unequip_weapon":
                    UnequipWeapon(session);
                    break;

                // Drag a primary item onto the primary equipped slot (replaces the weapon).
                case "equip_primary":
                    EquipPrimary(data, session);
                    break;

                // Drag/double-click the primary item out of the primary slot.
                case "unequip_primary":
                    UnequipPrimary(session);
                    break;

                // Drag a shield onto the secondary equipped slot.
                case "equip_shield":
                    EquipShield(data, session);
                    break;

                // Drag/double-click the shield out of the secondary slot.
                case "unequip_shield":
                    UnequipShield(session);
                    break;

                // Drag a slot onto the trashbin — destroy the whole stack.
                case "disband_item":
                    DisbandItem(data, session);
                    break;

                case "open_wanted":
                    session.SendPacket(new WantedListComposer());
                    break;

                case "open_corps":
                case "request_corps":
                    WebOverlay.SendCorporations(session);
                    break;

                // Organization button in the gaz-js left menu — toggle the Nitro gang window.
                case "open_gang":
                    session.SendPacket(new Plus.Communication.Packets.Outgoing.Roleplay.Gang.RPGangToggleComposer());
                    break;

                // A line bought from the :offer / stock_vendor buy window.
                case "buy_stock":
                    BuyStock(data, session);
                    break;

                case "close_offer":
                    RpOfferService.Clear(session.GetHabbo().Id);
                    break;

                // Accept/decline of a :hire group invite popup.
                case "group_invite_response":
                    if (TryGetInt(data, "inviteId", out int inviteId))
                        GroupInviteService.Respond(session, inviteId, GetBool(data, "accepted"));
                    break;

                // ── My Items ────────────────────────────────────────────
                case "open_my_items":
                case "request_my_items":
                    WebOverlay.SendMyItems(session);
                    break;

                case "toggle_skill":
                    ToggleSkill(data, session);
                    break;

                case "equip_bubble":
                    EquipBubble(data, session);
                    break;

                case "toggle_weapon_skin":
                    ToggleWeaponSkin(data, session);
                    break;

                // The user clicked the lock/unlock toggle on their current target.
                case "lock_target": {
                        bool locked = GetBool(data, "lock");
                        TargetLockService.SetLock(session.GetHabbo(), locked);
                        Habbo clicked = TargetLockService.GetClickedHabbo(session.GetHabbo().Id);
                        bool isLocked = TargetLockService.IsLocked(session.GetHabbo().Id);
                        session.SendPacket(new TargetLockComposer(clicked?.Username ?? string.Empty, isLocked));
                        break;
                    }

                // ── Macros ──────────────────────────────────────────────
                case "request_macro_set":
                    SendMacroSet(session);
                    break;

                case "request_macro_list":
                    PlusEnvironment.GetGame().GetMacroManager().EnsureDefaultSets(session.GetHabbo().Id);
                    SendMacroList(session);
                    break;

                case "create_macro":
                    CreateMacro(data, session);
                    break;

                case "save_macro":
                    SaveMacro(data, session);
                    break;

                case "select_macro":
                    SelectMacro(data, session);
                    break;

                case "delete_macro":
                    DeleteMacro(data, session);
                    break;

                case "set_macro_enabled":
                    PlusEnvironment.GetGame().GetMacroManager().SetEnabled(session.GetHabbo().Id, GetBool(data, "enabled"));
                    SendMacroSet(session);
                    break;

                // A bound key was pressed in the overlay — run its text as if typed in chat.
                case "macro_execute":
                    ExecuteMacro(data, session);
                    break;

                default:
                    // Unknown header – ignore silently
                    break;
            }
        }

        // A toggle changed in the gaz-js Settings overlay: {key, value}. We apply it, persist,
        // run any behaviour hook, then echo the full settings back so the client reflects any
        // server-side veto (e.g. auto-salary requires a bank account).
        private static void SetSetting(JsonElement data, GameClient session)
        {
            Habbo habbo = session?.GetHabbo();
            Plus.HabboHotel.Roleplay.Settings.UserRpSettings settings = habbo?.GetRpSettings();
            if (settings == null)
                return;

            string key = GetString(data, "key");
            if (string.IsNullOrWhiteSpace(key))
                return;

            bool value = GetBool(data, "value");

            switch (key) {
                case "clickThrough":
                    settings.ClickThrough = value;
                    habbo.ClickThrough = value;
                    // Mirror the state to the client without a whisper (silent from settings).
                    session.SendPacket(new GuideSessionPartnerIsPlayingComposer(value));
                    break;

                case "autoSalary":
                    // Can only be enabled while the user owns a bank account.
                    settings.AutoSalaryDeposit = value && habbo.GetBankAccount() != null;
                    break;

                case "furnitureTrading":
                    settings.FurnitureTrading = value;
                    break;

                case "autoEvent":
                    settings.AutoEventTransfer = value;
                    break;

                case "voiceChat":
                    settings.VoiceChat = value;
                    break;

                case "livefeed":
                    settings.Livefeed = value;
                    break;

                case "alerts":
                    settings.Alerts = value;
                    break;

                case "linkWarning":
                    settings.LinkWarning = value;
                    session.SendPacket(new ChatLinkConfigComposer(session));
                    break;

                case "chatHigher":
                    settings.ChatHigher = value;
                    settings.Save();
                    session.SendPacket(new ClientVisualSettingsComposer(session));
                    WebOverlay.SendSettings(session);
                    return;

                case "dragRooms":
                    settings.DragRooms = value;
                    settings.Save();
                    session.SendPacket(new ClientVisualSettingsComposer(session));
                    WebOverlay.SendSettings(session);
                    return;

                case "fpsMax":
                    settings.FpsMax = UserRpSettings.ClampFps(TryGetInt(data, "value", out int fps) ? fps : UserRpSettings.FpsMaxDefault);
                    settings.Save();
                    session.SendPacket(new ClientVisualSettingsComposer(session));
                    WebOverlay.SendSettings(session);
                    return;

                default:
                    return;
            }

            settings.Save();

            // Echo the authoritative state back (covers vetoed toggles like auto-salary).
            WebOverlay.SendSettings(session);
        }

        // Equip/unequip a skill from the My Items panel. Habbo.ToggleSkill() enforces the
        // "unlocked + max 4 equipped" rules and carries the skill-7 health handling, so the
        // overlay gets exactly the same behaviour as the :toggleskill command.
        private static void ToggleSkill(JsonElement data, GameClient session)
        {
            if (!TryGetInt(data, "id", out int skillId))
                return;

            Habbo habbo = session.GetHabbo();
            if (habbo?.GetRpSkills() == null)
                return;

            if (habbo.ToggleSkill(skillId)) {
                habbo.SaveRpSkills();

                Skill skill = PlusEnvironment.GetSkillManager().GetSkillById(skillId);
                if (skill != null)
                    session.SendWhisper($"{skill.Name} is now {(habbo.GetRpSkills().IsEquipped(skillId) ? "equipped" : "unequipped")}.", 1);
            } else {
                session.SendWhisper("Skill couldn't be toggled. Please note you need to have the skill unlocked and the limit is " +
                    UserRpSkills.MaxEquippedSkills + ".", 1);
            }

            // Always refresh — the panel must snap back if the toggle was rejected.
            WebOverlay.SendMyItems(session);
        }

        // Equip an owned chat bubble (id 0 = back to the default bubble).
        private static void EquipBubble(JsonElement data, GameClient session)
        {
            if (!TryGetInt(data, "id", out int bubbleId))
                return;

            Habbo habbo = session.GetHabbo();
            if (habbo == null)
                return;

            if (!habbo.HasChatBubble(bubbleId)) {
                session.SendWhisper("You do not own that chat bubble.", 1);
                WebOverlay.SendMyItems(session);
                return;
            }

            // Id 0 clears any custom bubble. It has a room_chat_styles row for the overlay's
            // preview, but skip the checks anyway so a misconfigured row can never leave someone
            // stuck on a bubble with no way back to the default. Matches :bubble.
            if (bubbleId != 0) {
                if (!PlusEnvironment.GetGame().GetChatManager().GetChatStyles().TryGetStyle(bubbleId, out ChatStyle style)) {
                    session.SendWhisper("That chat bubble does not exist.", 1);
                    WebOverlay.SendMyItems(session);
                    return;
                }

                if (!string.IsNullOrEmpty(style.RequiredRight) && !habbo.GetPermissions().HasRight(style.RequiredRight)) {
                    session.SendWhisper("You cannot use this bubble due to a rank requirement, sorry!", 1);
                    WebOverlay.SendMyItems(session);
                    return;
                }
            }

            habbo.CustomBubbleId = bubbleId;

            // Mirror onto the live room user so the next line of chat uses it immediately.
            RoomUser roomUser = habbo.CurrentRoom?.GetRoomUserManager()?.GetRoomUserByHabbo(habbo.Id);
            if (roomUser != null)
                roomUser.LastBubble = bubbleId;

            WebOverlay.SendMyItems(session);
        }

        // Equip/unequip an owned weapon skin. Mirrors the :weaponskin command, including
        // re-applying the effect when the skin belongs to the currently active weapon.
        private static void ToggleWeaponSkin(JsonElement data, GameClient session)
        {
            if (!TryGetInt(data, "id", out int userSkinId))
                return;

            Habbo habbo = session.GetHabbo();
            UserRpWeaponSkins weaponSkins = habbo?.GetRpWeaponSkins();
            UserWeaponSkin userSkin = weaponSkins?.GetSkin(userSkinId);
            if (userSkin == null) {
                session.SendWhisper("You do not own that skin.", 1);
                WebOverlay.SendMyItems(session);
                return;
            }

            if (!weaponSkins.ToggleSkin(userSkinId)) {
                session.SendWhisper("That skin could not be toggled.", 1);
                WebOverlay.SendMyItems(session);
                return;
            }

            bool equipped = userSkin.Equipped;
            UserWeapon activeWeapon = habbo.GetRpStats()?.ActiveWeapon();
            if (activeWeapon?.WeaponData != null && activeWeapon.WeaponId == userSkin.SkinData.WeaponId)
                habbo.Effects().ApplyEffect(equipped ? userSkin.SkinData.EffectId : activeWeapon.WeaponData.Effect);

            // The shout is room-scoped; equipping from the overlay while roomless is still allowed.
            RoomUser roomUser = habbo.CurrentRoom?.GetRoomUserManager()?.GetRoomUserByHabbo(habbo.Id);
            if (roomUser != null) {
                habbo.CurrentRoom.SendPacket(new ShoutComposer(roomUser.VirtualId,
                    (equipped ? "*equips the " : "*unequips the ") + userSkin.SkinData.Name + " skin*",
                    0, habbo.CustomBubbleId, isRpAction: true));
            }

            WebOverlay.SendMyItems(session);
        }

        private static void SendMacroList(GameClient session)
        {
            MacroManager manager = PlusEnvironment.GetGame().GetMacroManager();
            WebOverlay.SendMacroList(session, manager.GetUserMacros(session.GetHabbo().Id).Values);
        }

        private static void SendMacroSet(GameClient session)
        {
            MacroManager manager = PlusEnvironment.GetGame().GetMacroManager();
            int userId = session.GetHabbo().Id;
            WebOverlay.SendMacroSet(session, manager.GetSelected(userId), manager.IsEnabled(userId));
        }

        private static void OpenInventory(GameClient session) => SendInventory(session, true);

        internal static void RefreshInventory(GameClient session)
        {
            if (session?.GetHabbo() == null)
                return;

            SendInventory(session, false);
        }

        private static string ItemBlockReason(RpItemData data, bool dead, bool passive, bool cuffed)
        {
            if (data == null)
                return "";

            if (data.IsNone)
                return "inert";

            if (dead && !data.UsableWhileDead)
                return "dead";

            if (cuffed && !data.UsableWhileCuffed && !data.Has("handpick"))
                return "cuffed";

            if (passive && data.BlockedWhilePassive)
                return "passive";

            return "";
        }

        private static void SendInventory(GameClient session, bool open)
        {
            Habbo habbo = session.GetHabbo();
            if (habbo == null)
                return;

            // Keep the equipped shield's stored durability in sync with the live stat.
            habbo.GetRpItems()?.SyncEquippedShield(habbo);

            // Passive mode is surfaced so the overlay can grey out / block equipping combat gear.
            bool passive = habbo.GetRpStats()?.PassiveMode == true;
            bool dead = habbo.GetRpStats()?.IsDead == true;
            bool cuffed = PlusEnvironment.GetPoliceManager().IsCuffed(habbo.Id);

            List<object> items = [];

            // Primary equipped slot (0): a primary item if one is equipped, otherwise the weapon.
            UserRpItem equippedPrimary = habbo.GetRpItems()?.GetEquippedPrimaryItem();
            if (equippedPrimary?.ItemData != null) {
                bool hasBar = equippedPrimary.ItemData.MaxDurability > 0;
                items.Add(new
                {
                    id = equippedPrimary.Id,
                    kind = "primary",
                    item = equippedPrimary.ItemData.Name ?? "",
                    equipped = true,
                    equippedSlot = 0,
                    image = equippedPrimary.ItemData.ImageUrl ?? "",
                    weapon = hasBar,
                    rarity = equippedPrimary.ItemData.Rarity,
                    blockedReason = ItemBlockReason(equippedPrimary.ItemData, dead, passive, cuffed),
                    health = hasBar ? (int)Math.Round(100.0 * equippedPrimary.EffectiveDurability / Math.Max(1, equippedPrimary.ItemData.MaxDurability)) : 100
                });
            } else {
                UserWeapon weapon = habbo.GetRpWeapons()?.GetActiveWeapon();
                if (weapon?.WeaponData != null && weapon.WeaponData.Id != 0) {
                    items.Add(new
                    {
                        id = -1000 - weapon.Id,
                        userWeaponId = weapon.Id,
                        weaponId = weapon.WeaponId,
                        kind = "weapon",
                        item = weapon.WeaponData.Name ?? "",
                        equipped = true,
                        equippedSlot = 0,
                        image = weapon.WeaponData.Image ?? "",
                        weapon = true,
                        passiveBlock = true,
                        // Weapons aren't RP items, so the reason is derived from state alone. A
                        // weapon can never be used while cuffed, so cuffs always block it.
                        blockedReason = dead ? "dead" : (cuffed ? "cuffed" : (passive ? "passive" : "")),
                        rarity = (int)weapon.WeaponData.Rarity,
                        health = weapon.WeaponData.Durability <= 0 ? 100
                            : (int)Math.Round(100.0 * weapon.DurabilityLeft / weapon.WeaponData.Durability)
                    });
                }
            }

            // Secondary equipped slot (1): the equipped secondary (shield) item (durability driven).
            UserRpItem equippedShield = habbo.GetRpItems()?.GetEquippedShield();
            if (equippedShield?.ItemData != null) {
                // A suicide vest is single-use — it has no durability, so it renders no bar.
                bool isSuicide = equippedShield.ItemData.IsSuicideVest;
                int maxShield = Math.Max(1, equippedShield.ItemData.MaxDurability);
                items.Add(new
                {
                    id = equippedShield.Id,
                    kind = "secondary",
                    item = equippedShield.ItemData.Name ?? "Shield",
                    equipped = true,
                    equippedSlot = 1,
                    image = equippedShield.ItemData.ImageUrl ?? "",
                    weapon = !isSuicide,
                    shield = !isSuicide,
                    suicide = isSuicide,
                    passiveBlock = isSuicide || equippedShield.ItemData.BlockedWhilePassive,
                    blockedReason = ItemBlockReason(equippedShield.ItemData, dead, passive, cuffed),
                    rarity = equippedShield.ItemData.Rarity,
                    health = isSuicide ? 100 : (int)Math.Round(100.0 * equippedShield.EffectiveDurability / maxShield)
                });
            }

            // Normal grid: merged + stacked items and (non-active) weapons.
            RpInventoryEntry[] view = RpInventory.BuildSlotView(habbo, 10);
            for (int slot = 0; slot < view.Length; slot++) {
                RpInventoryEntry entry = view[slot];
                if (entry == null)
                    continue;

                if (entry.Kind == "weapon") {
                    UserWeapon w = entry.Weapon;
                    // Lock the resolved position so consuming items leaves a blank slot
                    // instead of reflowing the rest of the grid.
                    habbo.GetRpWeapons()?.SetSlot(w.Id, slot);
                    items.Add(new
                    {
                        id = -100000 - w.Id,
                        userWeaponId = w.Id,
                        weaponId = w.WeaponId,
                        kind = "weapon",
                        item = w.WeaponData?.Name ?? "",
                        equipped = false,
                        image = w.WeaponData?.Image ?? "",
                        weapon = true,
                        canEquip = true,
                        passiveBlock = true,
                        blockedReason = dead ? "dead" : (cuffed ? "cuffed" : (passive ? "passive" : "")),
                        rarity = w.WeaponData?.Rarity ?? 0,
                        slot,
                        health = w.WeaponData == null || w.WeaponData.Durability <= 0 ? 100
                            : (int)Math.Round(100.0 * w.DurabilityLeft / w.WeaponData.Durability)
                    });
                    continue;
                }

                UserRpItem rep = entry.Representative;
                // Lock the resolved position (see weapon branch above).
                habbo.GetRpItems()?.SetSlot(rep.Id, slot);
                bool isSecondary = entry.Kind == "secondary";
                bool isPrimary = entry.Kind == "primary";
                bool hasBar = rep.ItemData != null && rep.ItemData.MaxDurability > 0;
                int health = hasBar
                    ? (int)Math.Round(100.0 * rep.EffectiveDurability / Math.Max(1, rep.ItemData.MaxDurability))
                    : 100;

                items.Add(new
                {
                    id = rep.Id,
                    kind = entry.Kind,
                    item = rep.ItemData?.Name ?? "",
                    equipped = false,
                    image = rep.ItemData?.ImageUrl ?? "",
                    weapon = hasBar, // shields / durability items render a bar
                    shield = isSecondary,
                    // Tells the overlay whether this item may be dropped on an equip slot.
                    canEquip = isSecondary || isPrimary,
                    suicide = rep.ItemData?.IsSuicideVest ?? false,
                    passiveBlock = rep.ItemData?.BlockedWhilePassive ?? false,
                    blockedReason = ItemBlockReason(rep.ItemData, dead, passive, cuffed),
                    rarity = rep.ItemData?.Rarity ?? 1,
                    slot,
                    count = entry.Count,
                    stackLimit = rep.ItemData?.StackLimit ?? 1,
                    ids = entry.Stack.Select(x => x.Id).ToArray(),
                    health
                });
            }

            WebOverlay.Send(session, "inventory", new { items, open, passive, dead, cuffed });
        }

        // Unequip the active weapon — it returns to the grid, so it needs a free slot.
        private static void UnequipWeapon(GameClient session)
        {
            Habbo habbo = session.GetHabbo();
            if (habbo == null)
                return;

            UserWeapon active = habbo.GetRpWeapons()?.GetActiveWeapon();
            bool hasRealWeapon = active?.WeaponData != null && active.WeaponData.Id != 0;
            if (hasRealWeapon && RpInventory.UsedSlots(habbo, 10) >= 10) {
                session.SendWhisper("You don't have a free inventory slot — disband something first.", 1);
                return;
            }

            RpWeaponService.Equip(session, 0, out _);
            OpenInventory(session);
        }

        // Unequip the shield — it returns to the grid, so it needs a free slot.
        private static void UnequipShield(GameClient session)
        {
            Habbo habbo = session.GetHabbo();
            UserRpItems items = habbo?.GetRpItems();
            if (items == null)
                return;

            UserRpItem equippedSecondary = items.GetEquippedShield();
            if (equippedSecondary == null) {
                OpenInventory(session);
                return;
            }

            // Unequipping (drag out / take off) NEVER detonates — a suicide vest is only triggered
            // by an explicit use (double-click), handled via UseItem. This lets it be dragged away
            // safely instead of blowing up the moment it's touched.
            if (RpInventory.UsedSlots(habbo, 10) >= 10) {
                session.SendWhisper("You don't have a free inventory slot — disband something first.", 1);
                return;
            }

            items.UnequipShield(habbo);
            OpenInventory(session);
        }

        // Drag a weapon onto the primary equipped slot — make it the active weapon.
        private static void EquipWeapon(JsonElement data, GameClient session)
        {
            if (!TryGetInt(data, "userWeaponId", out int userWeaponId))
                return;

            // Pre-emptively surface the rejection reason (passive / cuffed / on-duty) so the
            // player is told why the weapon didn't equip instead of it silently snapping back.
            if (!RpWeaponService.Equip(session, userWeaponId, out string error) && !string.IsNullOrEmpty(error))
                session.SendWhisper(error, 1);
            OpenInventory(session);
        }

        // Drag a primary item onto the primary equipped slot — replaces the weapon.
        private static void EquipPrimary(JsonElement data, GameClient session)
        {
            if (!TryGetInt(data, "id", out int itemId))
                return;

            session.GetHabbo()?.GetRpItems()?.EquipPrimaryItem(itemId, session.GetHabbo());
            OpenInventory(session);
        }

        // Unequip the primary item — it returns to the grid, so it needs a free slot.
        private static void UnequipPrimary(GameClient session)
        {
            Habbo habbo = session.GetHabbo();
            UserRpItems items = habbo?.GetRpItems();
            if (items == null)
                return;

            if (items.GetEquippedPrimaryItem() == null) {
                OpenInventory(session);
                return;
            }

            if (RpInventory.UsedSlots(habbo, 10) >= 10) {
                session.SendWhisper("You don't have a free inventory slot — disband something first.", 1);
                return;
            }

            items.UnequipPrimaryItem(habbo);
            OpenInventory(session);
        }

        // Drag a shield onto the secondary equipped slot.
        private static void EquipShield(JsonElement data, GameClient session)
        {
            if (!TryGetInt(data, "id", out int itemId))
                return;

            session.GetHabbo()?.GetRpItems()?.EquipShield(itemId, session.GetHabbo());
            OpenInventory(session);
        }

        // Trashbin drop — destroy the whole stack at the dragged slot.
        private static void DisbandItem(JsonElement data, GameClient session)
        {
            Habbo habbo = session.GetHabbo();
            UserRpItems items = habbo?.GetRpItems();
            if (items == null)
                return;

            UserRpItem equippedShieldBefore = items.GetEquippedShield();

            // A weapon is identified by kind/userWeaponId — handle it first so the synthetic
            // negative display id (which never matches a real item) can't send us down the item
            // path and silently no-op. This is what let equipped weapons be trashed but not the
            // brand-new grid ones.
            bool isWeapon = GetString(data, "kind") == "weapon";
            if (isWeapon && TryGetInt(data, "userWeaponId", out int weaponId)) {
                habbo.GetRpWeapons()?.RemoveWeapon(weaponId);
            }
            // Prefer an explicit list of ids (a stack); fall back to a single id.
            else if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("ids", out JsonElement idsEl) && idsEl.ValueKind == JsonValueKind.Array) {
                foreach (JsonElement e in idsEl.EnumerateArray()) {
                    if (e.TryGetInt32(out int iid)) {
                        UserRpItem it = items.GetItem(iid);
                        if (it != null)
                            items.Remove(it);
                    }
                }
            } else if (TryGetInt(data, "id", out int id)) {
                UserRpItem it = items.GetItem(id);
                if (it != null)
                    items.Remove(it);
                else if (TryGetInt(data, "userWeaponId", out int uwid))
                    habbo.GetRpWeapons()?.RemoveWeapon(uwid);
            }

            // Destroying the equipped shield must also drop the live shield stat.
            if (equippedShieldBefore != null && items.GetItem(equippedShieldBefore.Id) == null) {
                UserRpStats stats = habbo.GetRpStats();
                if (stats != null) {
                    stats.Shield = 0;
                    habbo.TrySendUserStatsUpdate(true);
                }
            }

            // Disbanding an equipped clothing item must restore the wearer's real figure.
            RpItemClothingService.Refresh(habbo);

            habbo.SaveRpItems();
            habbo.SaveRpWeapons();
            OpenInventory(session);
        }

        // Use/activate an owned RP item by its user-item id (double-click in the overlay).
        private static void UseItem(JsonElement data, GameClient session)
        {
            if (!TryGetInt(data, "id", out int userItemId))
                return;

            Habbo habbo = session.GetHabbo();
            UserRpItems items = habbo?.GetRpItems();
            if (items?.GetItem(userItemId) == null)
                return;

            items.Use(userItemId, habbo);
            OpenInventory(session); // refresh the panel (durability/use count/removal)
        }

        // Persist the new grid positions sent after a drag & drop reorder.
        private static void SaveInventory(JsonElement data, GameClient session)
        {
            Habbo habbo = session.GetHabbo();
            UserRpItems items = habbo?.GetRpItems();
            UserRpWeapons weapons = habbo?.GetRpWeapons();
            if (items == null || data.ValueKind != JsonValueKind.Object ||
                !data.TryGetProperty("slots", out JsonElement slots) || slots.ValueKind != JsonValueKind.Array)
                return;

            foreach (JsonElement entry in slots.EnumerateArray()) {
                if (entry.ValueKind != JsonValueKind.Object || !TryGetInt(entry, "slot", out int slot))
                    continue;

                if (GetString(entry, "kind") == "weapon") {
                    if (weapons != null && TryGetInt(entry, "userWeaponId", out int uwid))
                        weapons.SetSlot(uwid, slot);
                } else if (TryGetInt(entry, "id", out int id) && id > 0) {
                    items.SetSlot(id, slot);
                }
            }
        }

        private static void ExecuteMacro(JsonElement data, GameClient session)
        {
            string text = GetString(data, "text");
            if (string.IsNullOrWhiteSpace(text))
                return;

            Habbo habbo = session.GetHabbo();
            if (habbo == null || !habbo.InRoom)
                return;

            Room room = habbo.CurrentRoom;
            RoomUser user = room?.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (user == null)
                return;

            text = StringCharFilter.Escape(text);
            if (text.Length > 110)
                text = text.Substring(0, 110);

            user.UnIdle();

            // Keybinds act on the user's current target — the clicked user when nothing is
            // locked, otherwise the locked target. A bare command (":hit") auto-fills that
            // target as its argument; a command using the placeholder (":hit x") has the
            // placeholder swapped for it. Any other explicit argument is left untouched.
            if (text.StartsWith(":", StringComparison.CurrentCulture)) {
                Habbo target = TargetLockService.GetTarget(habbo);
                int space = text.IndexOf(' ');

                if (space < 0) {
                    if (target != null)
                        text += " " + target.Username;
                } else {
                    string command = text.Substring(0, space);
                    string argument = text.Substring(space + 1).Trim();

                    // Only the placeholder is substituted; a real username stays as typed.
                    // With no target, fall back to the bare command so the command's own
                    // targeting/error handling takes over.
                    if (string.Equals(argument, MacroTargetPlaceholder, StringComparison.OrdinalIgnoreCase))
                        text = target != null ? command + " " + target.Username : command;
                }

                if (PlusEnvironment.GetGame().GetChatManager().GetCommands().Parse(session, text))
                    return;
            }

            user.OnChat(user.LastBubble, text, false);
        }

        private static void CreateMacro(JsonElement data, GameClient session)
        {
            string name = GetString(data, "name");
            if (string.IsNullOrWhiteSpace(name))
                return;

            PlusEnvironment.GetGame().GetMacroManager().CreateMacro(session.GetHabbo().Id, name);
            SendMacroList(session);
        }

        private static void SaveMacro(JsonElement data, GameClient session)
        {
            if (!TryGetInt(data, "id", out int id))
                return;

            string name = GetString(data, "name");
            Dictionary<string, string> sets = new();
            if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("sets", out JsonElement setsEl) && setsEl.ValueKind == JsonValueKind.Object) {
                foreach (JsonProperty prop in setsEl.EnumerateObject())
                    sets[prop.Name] = prop.Value.GetString() ?? "";
            }

            PlusEnvironment.GetGame().GetMacroManager().SaveMacro(session.GetHabbo().Id, id, name, sets);
            SendMacroSet(session);
        }

        private static void SelectMacro(JsonElement data, GameClient session)
        {
            if (!TryGetInt(data, "id", out int id))
                return;

            PlusEnvironment.GetGame().GetMacroManager().SetSelected(session.GetHabbo().Id, id);
            SendMacroSet(session);
        }

        private static void DeleteMacro(JsonElement data, GameClient session)
        {
            if (!TryGetInt(data, "id", out int id))
                return;

            PlusEnvironment.GetGame().GetMacroManager().DeleteMacro(session.GetHabbo().Id, id);
            SendMacroList(session);
        }

        private static void BuyStock(JsonElement data, GameClient session)
        {
            if (!TryGetInt(data, "offerId", out int offerId) ||
                !TryGetInt(data, "itemId", out int itemId) ||
                !TryGetInt(data, "amount", out int amount))
                return;

            string stockType = GetString(data, "stockType");
            if (string.IsNullOrEmpty(stockType))
                return;

            RpOfferService.HandlePurchase(session, offerId, stockType, itemId, amount);
        }

        private static string GetString(JsonElement data, string key)
        {
            return data.ValueKind == JsonValueKind.Object && data.TryGetProperty(key, out JsonElement el) && el.ValueKind == JsonValueKind.String
                ? el.GetString()
                : null;
        }

        private static bool GetBool(JsonElement data, string key)
        {
            if (data.ValueKind != JsonValueKind.Object || !data.TryGetProperty(key, out JsonElement el))
                return false;

            return el.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => bool.TryParse(el.GetString(), out bool b) && b,
                JsonValueKind.Number => el.TryGetInt32(out int n) && n != 0,
                _ => false
            };
        }

        private static bool TryGetInt(JsonElement data, string key, out int value)
        {
            value = 0;
            if (data.ValueKind != JsonValueKind.Object || !data.TryGetProperty(key, out JsonElement el))
                return false;

            if (el.ValueKind == JsonValueKind.Number && el.TryGetInt32(out value))
                return true;

            return el.ValueKind == JsonValueKind.String && int.TryParse(el.GetString(), out value);
        }
    }
}