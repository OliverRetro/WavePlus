using System;
using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Roleplay.RpItem.Item;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Roleplay.RpItem
{
    public sealed class RpInventoryEntry
    {
        // "item", "shield" or "weapon".
        public string Kind { get; set; }

        // Preferred grid position (0-based), -1 when unassigned.
        public int Slot { get; set; } = -1;

        // Populated for weapon entries.
        public UserWeapon Weapon { get; set; }

        // Populated for item/shield entries (one element = unstacked, many = a stack).
        public List<UserRpItem> Stack { get; set; }

        public RpItemData ItemData => Stack != null && Stack.Count > 0 ? Stack[0].ItemData : null;
        public UserRpItem Representative => Stack != null && Stack.Count > 0 ? Stack[0] : null;
        public int Count => Stack?.Count ?? 1;
    }

    public static class RpInventory
    {
        public static List<RpInventoryEntry> BuildEntries(Habbo habbo)
        {
            List<RpInventoryEntry> entries = new();

            UserRpItems items = habbo?.GetRpItems();
            if (items != null) {
                // Stackable items collapse by item-id into chunks of stack_limit; everything
                // else (non-stackable, shields with durability) is a single entry.
                Dictionary<int, List<UserRpItem>> stackable = new();
                foreach (UserRpItem item in items.Items) {
                    if (item.Equipped) // equipped shield lives in the secondary slot
                        continue;

                    if (item.ItemData != null && item.ItemData.IsStackable) {
                        if (!stackable.TryGetValue(item.ItemId, out List<UserRpItem> list))
                            stackable[item.ItemId] = list = new List<UserRpItem>();
                        list.Add(item);
                    } else {
                        entries.Add(new RpInventoryEntry
                        {
                            Kind = item.ItemData == null ? "item"
                                : item.ItemData.IsPrimary ? "primary"
                                : item.ItemData.IsSecondary ? "secondary"
                                : "item",
                            Stack = new List<UserRpItem> { item },
                            Slot = item.Slot
                        });
                    }
                }

                foreach (List<UserRpItem> group in stackable.Values) {
                    int limit = group[0].ItemData.StackLimit;
                    List<UserRpItem> ordered = group
                        .OrderBy(x => x.Slot < 0 ? int.MaxValue : x.Slot)
                        .ThenBy(x => x.Id)
                        .ToList();

                    for (int i = 0; i < ordered.Count; i += limit) {
                        List<UserRpItem> chunk = ordered.Skip(i).Take(limit).ToList();
                        entries.Add(new RpInventoryEntry { Kind = "item", Stack = chunk, Slot = chunk[0].Slot });
                    }
                }
            }

            UserRpWeapons weapons = habbo?.GetRpWeapons();
            if (weapons != null) {
                int activeId = weapons.ActiveWeaponId;
                foreach (UserWeapon weapon in weapons.Weapons) {
                    if (weapon.IsDefault || weapon.Id == activeId)
                        continue; // active weapon is shown in the primary equipped slot

                    entries.Add(new RpInventoryEntry { Kind = "weapon", Weapon = weapon, Slot = weapon.Slot });
                }
            }

            return entries;
        }

        public static RpInventoryEntry[] BuildSlotView(Habbo habbo, int slotCount)
        {
            RpInventoryEntry[] view = new RpInventoryEntry[slotCount];
            List<RpInventoryEntry> overflow = new();

            foreach (RpInventoryEntry entry in BuildEntries(habbo)) {
                if (entry.Slot >= 0 && entry.Slot < slotCount && view[entry.Slot] == null)
                    view[entry.Slot] = entry;
                else
                    overflow.Add(entry);
            }

            // Slots held by equipped items are reserved so they return to the same spot when
            // unequipped instead of being stolen by an unassigned item.
            HashSet<int> reserved = GetReservedSlots(habbo, slotCount);

            // Deterministic order: saved items (positive ids) first in acquisition order, brand
            // new items (negative ids) last. Keeps placement stable between opens — never random.
            overflow = overflow
                .OrderBy(e => EntryId(e) > 0 ? EntryId(e) : int.MaxValue)
                .ThenBy(EntryId)
                .ToList();

            foreach (RpInventoryEntry entry in overflow) {
                int next = 0;
                while (next < slotCount && (view[next] != null || reserved.Contains(next)))
                    next++;

                // Grid full of real + reserved slots — fall back to any genuinely empty slot.
                if (next >= slotCount)
                    next = Array.FindIndex(view, s => s == null);

                if (next < 0)
                    break;

                view[next] = entry;
            }

            return view;
        }

        private static int EntryId(RpInventoryEntry entry) =>
            entry.Kind == "weapon" ? entry.Weapon?.Id ?? 0 : entry.Representative?.Id ?? 0;

        private static HashSet<int> GetReservedSlots(Habbo habbo, int slotCount)
        {
            HashSet<int> reserved = new();

            UserRpItems items = habbo?.GetRpItems();
            if (items != null)
                foreach (UserRpItem it in items.Items)
                    if (it.Equipped && it.Slot >= 0 && it.Slot < slotCount)
                        reserved.Add(it.Slot);

            UserRpWeapons weapons = habbo?.GetRpWeapons();
            if (weapons != null && weapons.ActiveWeaponId != 0) {
                UserWeapon active = weapons.GetWeapon(weapons.ActiveWeaponId);
                if (active != null && active.Slot >= 0 && active.Slot < slotCount)
                    reserved.Add(active.Slot);
            }

            return reserved;
        }

        public static int UsedSlots(Habbo habbo, int slotCount) =>
            BuildSlotView(habbo, slotCount).Count(x => x != null);
    }
}