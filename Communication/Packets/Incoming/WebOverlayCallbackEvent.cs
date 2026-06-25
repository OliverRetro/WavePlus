using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Plus.Communication.Packets.Outgoing.Overlay;
using Plus.Communication.Packets.Outgoing.Roleplay;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Macros;
using Plus.HabboHotel.Roleplay.RpItem.Item;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Roleplay.TargetLock;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming
{
    /// <summary>
    /// Receives JSON callbacks from the gaz-js web overlay (habblet bridge, client packet 314).
    /// Payload: {"header":"...","data":{...}}. Dispatches to the matching handler.
    /// </summary>
    internal class WebOverlayCallbackEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            // DIAGNOSTIC: proves packet 314 reached this handler at all.
            string payload = packet.PopString();
            Console.WriteLine("[WebOverlay] <-- packet 314 received, raw payload: " + payload);

            if (session?.GetHabbo() == null)
            {
                Console.WriteLine("[WebOverlay] aborted: session/habbo is null");
                return;
            }

            payload = payload.Replace("&#47;", "/");
            if (string.IsNullOrWhiteSpace(payload))
            {
                Console.WriteLine("[WebOverlay] aborted: empty payload");
                return;
            }

            try
            {
                using JsonDocument doc = JsonDocument.Parse(payload);
                JsonElement root = doc.RootElement;

                if (!root.TryGetProperty("header", out JsonElement headerEl))
                {
                    Console.WriteLine("[WebOverlay] aborted: no 'header' property in payload");
                    return;
                }

                string header = headerEl.GetString() ?? string.Empty;
                JsonElement data = root.TryGetProperty("data", out JsonElement d) ? d : default;

                Console.WriteLine("[WebOverlay] dispatching header: '" + header + "'");
                Dispatch(header, data, session);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[WebOverlay] malformed message: " + ex.Message);
            }
        }

        private static void Dispatch(string header, JsonElement data, GameClient session)
        {
            switch (header)
            {
                // The overlay finished loading / handshake — push everything it needs.
                case "sso":
                case "js_loaded":
                case "loaded":
                    WebOverlay.SendSessionData(session);
                    WebOverlay.SendHealth(session);
                    WebOverlay.SendShield(session);
                    WebOverlay.SendOnline(session, true);
                    PlusEnvironment.GetGame().GetMacroManager().EnsureDefaultSets(session.GetHabbo().Id);
                    SendMacroList(session);
                    SendMacroSet(session);
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

                case "open_wanted":
                    session.SendPacket(new WantedListComposer());
                    break;

                // The user clicked the lock/unlock toggle on their current target.
                case "lock_target":
                {
                    bool locked = GetBool(data, "lock");
                    TargetLockService.SetLock(session.GetHabbo(), locked);
                    Habbo clicked = TargetLockService.GetClickedHabbo(session.GetHabbo().Id);
                    session.SendPacket(new TargetLockComposer(clicked?.Username ?? string.Empty, locked));
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

        private static void OpenInventory(GameClient session)
        {
            Habbo habbo = session.GetHabbo();
            if (habbo == null)
                return;

            List<object> items = new();

            // Slot 0 (first equipped slot) is the user's active weapon.
            UserWeapon weapon = habbo.GetRpWeapons()?.GetActiveWeapon();
            if (weapon?.WeaponData != null && weapon.WeaponData.Id != 0)
            {
                items.Add(new
                {
                    id = -1000 - weapon.Id,
                    item = weapon.WeaponData.Name ?? "",
                    equipped = true,
                    equippedSlot = 0,
                    image = weapon.WeaponData.Image ?? "",
                    weapon = true,
                    health = weapon.WeaponData.Durability <= 0 ? 100
                        : (int)Math.Round(100.0 * weapon.DurabilityLeft / weapon.WeaponData.Durability)
                });
            }

            // Slot 1 is the user's shield (driven by current shield stat).
            int shield = habbo.GetRpStats()?.Shield ?? 0;
            if (shield > 0)
            {
                items.Add(new
                {
                    id = -2000,
                    item = "Shield",
                    equipped = true,
                    equippedSlot = 1,
                    image = "shield",
                    weapon = true,
                    health = shield
                });
            }

            // The remaining slots are normal RP items. Always send the panel even when the
            // user has no items component yet — the overlay needs the message to open.
            UserRpItems rpItems = habbo.GetRpItems();
            if (rpItems?.Items != null)
            {
                foreach (UserRpItem item in rpItems.Items)
                {
                    items.Add(new
                    {
                        id = item.Id,
                        item = item.ItemData?.Name ?? "",
                        equipped = false,
                        image = item.ItemData?.ImageUrl ?? "",
                        weapon = false,
                        slot = item.Slot
                    });
                }
            }

            WebOverlay.Send(session, "inventory", new { items });
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
            UserRpItems items = session.GetHabbo()?.GetRpItems();
            if (items == null || data.ValueKind != JsonValueKind.Object ||
                !data.TryGetProperty("slots", out JsonElement slots) || slots.ValueKind != JsonValueKind.Array)
                return;

            foreach (JsonElement entry in slots.EnumerateArray())
            {
                if (entry.ValueKind != JsonValueKind.Object)
                    continue;
                if (TryGetInt(entry, "id", out int id) && TryGetInt(entry, "slot", out int slot))
                    items.SetSlot(id, slot);
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

            // A keybind fires a bare command (e.g. ":hit"). Auto-fill the user's current
            // target — the clicked user when nothing is locked, otherwise the locked
            // target — as the command argument, so binds act on whoever is targeted.
            if (text.StartsWith(":", StringComparison.CurrentCulture))
            {
                if (!text.Contains(' '))
                {
                    Habbo target = TargetLockService.GetClickedHabbo(habbo.Id);
                    if (target != null)
                        text += " " + target.Username;
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
            if (data.ValueKind == JsonValueKind.Object && data.TryGetProperty("sets", out JsonElement setsEl) && setsEl.ValueKind == JsonValueKind.Object)
            {
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
