using System;
using System.Collections.Generic;
using System.Linq;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Roleplay.RpItem.Item
{
    public class UserRpItems
    {
        private readonly Dictionary<int, UserRpItem> _items;
        private readonly RpItemManager _itemManager;
        private readonly int _userId;

        private readonly HashSet<int> _removedIds = [];

        public UserRpItems(int userId, RpItemManager itemManager, IEnumerable<UserRpItem> items)
        {
            _userId = userId;
            _itemManager = itemManager;
            _items = items.ToDictionary(x => x.Id, x => x);
        }

        public IEnumerable<UserRpItem> Items => _items.Values;

        public IEnumerable<int> RemovedIds => _removedIds;

        public bool Dirty { get; private set; }

        public UserRpItem GetItem(int userItemId) =>
            _items.TryGetValue(userItemId, out UserRpItem item) ? item : null;

        public IEnumerable<UserRpItem> GetItemsByItemId(int itemId) =>
            _items.Values.Where(x => x.ItemId == itemId);

        public UserRpItem AddItem(int baseItemId)
        {
            if (!_itemManager.TryGetItem(baseItemId, out RpItemData itemData))
                return null;

            int nextId = _items.Count == 0 ? -1 : Math.Min(-1, _items.Keys.Min() - 1);
            UserRpItem userItem = new(nextId, _userId, baseItemId, 0, itemData);
            _items[userItem.Id] = userItem;
            MarkDirty();
            return userItem;
        }
        /// <summary>
        /// Builds the ordered slot view the overlay shows: items with an assigned slot are
        /// placed at that index, the rest fill the first free slots in insertion order.
        /// </summary>
        public UserRpItem[] GetSlotView(int slotCount)
        {
            UserRpItem[] view = new UserRpItem[slotCount];
            List<UserRpItem> overflow = new();

            foreach (UserRpItem item in _items.Values)
            {
                int slot = item.Slot;
                if (slot >= 0 && slot < slotCount && view[slot] == null)
                    view[slot] = item;
                else
                    overflow.Add(item);
            }

            int next = 0;
            foreach (UserRpItem item in overflow)
            {
                while (next < slotCount && view[next] != null)
                    next++;
                if (next >= slotCount)
                    break;
                view[next] = item;
            }

            return view;
        }

        /// <summary>Returns the item shown at a 0-based slot index, or null if empty.</summary>
        public UserRpItem GetItemAtSlot(int slotIndex, int slotCount)
        {
            if (slotIndex < 0 || slotIndex >= slotCount)
                return null;

            return GetSlotView(slotCount)[slotIndex];
        }

        /// <summary>Persists a new grid position for an owned item.</summary>
        public void SetSlot(int userItemId, int slot)
        {
            UserRpItem item = GetItem(userItemId);
            if (item == null || item.Slot == slot)
                return;

            item.Slot = slot;
            MarkDirty();
        }

        public bool Use(int userItemId, Habbo habbo)
        {
            if (habbo == null)
                return false;

            UserRpItem userItem = GetItem(userItemId);
            if (userItem == null)
                return false;

            RpItemData data = userItem.ItemData;
            UserRpStats stats = habbo.GetRpStats();
            Room room = habbo.CurrentRoom;
            RoomUser roomUser = room?.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);

            // give fine shyte
            if (stats != null)
                ApplyStatEffects(habbo, stats, data, roomUser);

            // an item that drops you to 0 health kills you (no attacker)
            bool died = stats != null && stats.Health <= 0 && !stats.IsDead && roomUser != null;
            if (died)
                stats.Kill(habbo, roomUser);

            // effect or handitem, if 1 exists (skipped if the item just killed us)
            if (!died && roomUser != null && (data.HanditemData != 0 || data.EffectData != 0))
                roomUser.ApplyTimedItemConsumption(TimeSpan.FromMilliseconds(data.ConsumptionDurationMs), data.EffectData, data.HanditemData);

            // uses
            ConsumeUse(userItem);

            habbo.TrySendUserStatsUpdate(true);
            return true;
        }

        private static void ApplyStatEffects(Habbo habbo, UserRpStats stats, RpItemData data, RoomUser roomUser)
        {
            if (data.Has("hp"))
                stats.Health = Math.Clamp(stats.Health + data.GetInt("hp"), 0, UserRpStats.GetMaxHealth(habbo));

            if (data.Has("energy"))
                stats.Energy = Math.Clamp(stats.Energy + data.GetInt("energy"), 0, UserRpStats.GetMaxEnergy(habbo));

            if (data.Has("shield"))
                stats.Shield = Math.Clamp(stats.Shield + data.GetInt("shield"), 0, 100);

            if (data.Has("hunger"))
                stats.Hunger = Math.Clamp(stats.Hunger + data.GetInt("hunger"), 0, UserRpStats.GetMaxHunger(habbo));

            if (data.Has("experience"))
                stats.Experience = Math.Clamp(stats.Experience + data.GetInt("experience"), 0, 5000);

            if (data.Has("hunger_saturation"))
                stats.NextHungerTick = DateTime.UtcNow.AddMinutes(RpItemData.BaseHungerTickMinutes * data.Get("hunger_saturation"));

            if (data.Has("fastwalk") && roomUser != null)
                roomUser.ApplyTimedFastwalk(TimeSpan.FromMilliseconds(data.Get("fastwalk") * RpItemData.TimeUnitMs));
        }

        private void ConsumeUse(UserRpItem userItem)
        {
            userItem.Uses++;

            if (userItem.Uses >= Math.Max(1, userItem.ItemData.MaxUses))
                Remove(userItem);
            else
                MarkDirty();
        }

        public void Remove(UserRpItem userItem)
        {
            if (userItem == null || !_items.Remove(userItem.Id))
                return;

            if (userItem.Id > 0)
                _removedIds.Add(userItem.Id);

            MarkDirty();
        }

        public void MarkDirty() => Dirty = true;

        public void MarkSaved()
        {
            Dirty = false;
            _removedIds.Clear();
        }

        public void RekeyItem(int oldId, int newId)
        {
            if (oldId == newId || !_items.TryGetValue(oldId, out UserRpItem item))
                return;

            _items.Remove(oldId);
            _items[newId] = item;
        }
    }
}
