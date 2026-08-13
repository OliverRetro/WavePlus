using System;
using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Incoming;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.Roleplay.Cooldowns;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Roleplay;

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
        public UserRpItem[] GetSlotView(int slotCount)
        {
            UserRpItem[] view = new UserRpItem[slotCount];
            List<UserRpItem> overflow = new();

            foreach (UserRpItem item in _items.Values) {
                int slot = item.Slot;
                if (slot >= 0 && slot < slotCount && view[slot] == null)
                    view[slot] = item;
                else
                    overflow.Add(item);
            }

            int next = 0;
            foreach (UserRpItem item in overflow) {
                while (next < slotCount && view[next] != null)
                    next++;
                if (next >= slotCount)
                    break;
                view[next] = item;
            }

            return view;
        }

        public UserRpItem GetItemAtSlot(int slotIndex, int slotCount)
        {
            if (slotIndex < 0 || slotIndex >= slotCount)
                return null;

            return GetSlotView(slotCount)[slotIndex];
        }

        public void SetSlot(int userItemId, int slot)
        {
            UserRpItem item = GetItem(userItemId);
            if (item == null || item.Slot == slot)
                return;

            item.Slot = slot;
            MarkDirty();
        }

        public UserRpItem GetEquippedShield() =>
            _items.Values.FirstOrDefault(x => x.Equipped && x.ItemData != null && x.ItemData.IsSecondary);

        public UserRpItem GetEquippedPrimaryItem() =>
            _items.Values.FirstOrDefault(x => x.Equipped && x.ItemData != null && x.ItemData.IsPrimary);

        public bool EquipShield(int userItemId, Habbo habbo)
        {
            UserRpItem item = GetItem(userItemId);
            if (item == null || item.ItemData == null || !item.ItemData.IsSecondary)
                return false;

            if (IsBlockedByDeath(habbo, item.ItemData))
                return false;

            if (IsBlockedByCuffs(habbo, item.ItemData))
                return false;

            if (item.ItemData.BlockedWhilePassive && habbo?.GetRpStats()?.PassiveMode == true) {
                habbo.GetClient()?.SendWhisper($"You can't equip {item.ItemData.Name} while passive!", 1);
                return false;
            }

            // Only replace other secondary items — leave the primary slot untouched.
            foreach (UserRpItem other in _items.Values)
                if (other.ItemData != null && other.ItemData.IsSecondary)
                    other.Equipped = false;

            item.Equipped = true;
            // Keep the item's grid slot so it returns to the same place when unequipped
            // (the slot is reserved while equipped — see RpInventory.BuildSlotView).

            // Swap in the item's clothing figure (if any) while it's worn.
            RpItemClothingService.Refresh(habbo);

            // Vests (standard body armour / suicide vests) do NOT feed the shield pool — their
            // durability lives on the item itself and is managed directly in combat.
            if (!item.ItemData.IsVest && !item.ItemData.IsSuicideVest) {
                UserRpStats stats = habbo?.GetRpStats();
                if (stats != null)
                    stats.Shield = item.EffectiveDurability;
            }

            MarkDirty();
            habbo?.TrySendUserStatsUpdate(true);
            return true;
        }

        public UserRpItem GetEquippedVest() =>
            _items.Values.FirstOrDefault(x => x.Equipped && x.ItemData != null && x.ItemData.IsVest);

        public UserRpItem GetEquippedSuicideVest() =>
            _items.Values.FirstOrDefault(x => x.Equipped && x.ItemData != null && x.ItemData.IsSuicideVest);

        public bool UnequipSuicideVest(Habbo habbo)
        {
            UserRpItem vest = GetEquippedSuicideVest();
            if (vest == null)
                return false;

            vest.Equipped = false;
            MarkDirty();
            RpItemClothingService.Refresh(habbo);
            return true;
        }

        public bool EquipPrimaryItem(int userItemId, Habbo habbo)
        {
            UserRpItem item = GetItem(userItemId);
            if (item == null || item.ItemData == null || !item.ItemData.IsPrimary)
                return false;

            if (IsBlockedByDeath(habbo, item.ItemData))
                return false;

            if (IsBlockedByCuffs(habbo, item.ItemData))
                return false;

            if (item.ItemData.BlockedWhilePassive && habbo?.GetRpStats()?.PassiveMode == true) {
                habbo.GetClient()?.SendWhisper($"You can't equip {item.ItemData.Name} while passive!", 1);
                return false;
            }

            foreach (UserRpItem other in _items.Values)
                if (other.ItemData != null && other.ItemData.IsPrimary)
                    other.Equipped = false;

            item.Equipped = true;
            // Keep the item's grid slot so it returns to the same place when unequipped
            // (the slot is reserved while equipped — see RpInventory.BuildSlotView).

            // The primary slot holds a weapon OR a primary item — never both.
            habbo?.GetRpWeapons()?.SetActiveWeaponId(0);

            MarkDirty();
            RpEffectService.Refresh(habbo);
            RpItemClothingService.Refresh(habbo);
            habbo?.TrySendUserStatsUpdate(true);
            return true;
        }

        public void UnequipPrimaryItem(Habbo habbo)
        {
            UserRpItem item = GetEquippedPrimaryItem();
            if (item == null)
                return;

            item.Equipped = false;
            MarkDirty();
            RpEffectService.Refresh(habbo);
            RpItemClothingService.Refresh(habbo);
            habbo?.TrySendUserStatsUpdate(true);
        }

        public void UnequipShield(Habbo habbo)
        {
            UserRpItem shield = GetEquippedShield();
            if (shield == null)
                return;

            // A vest keeps its durability on the item, not in the shield pool.
            if (shield.ItemData != null && (shield.ItemData.IsVest || shield.ItemData.IsSuicideVest)) {
                shield.Equipped = false;
                MarkDirty();
                RpItemClothingService.Refresh(habbo);
                habbo?.TrySendUserStatsUpdate(true);
                return;
            }

            UserRpStats stats = habbo?.GetRpStats();
            if (stats != null) {
                shield.DurabilityLeft = Math.Max(0, stats.Shield);
                stats.Shield = 0;
            }

            shield.Equipped = false;
            MarkDirty();
            RpItemClothingService.Refresh(habbo);
            habbo?.TrySendUserStatsUpdate(true);
        }

        public void SyncEquippedShield(Habbo habbo)
        {
            UserRpItem shield = GetEquippedShield();
            if (shield == null)
                return;

            // Vests don't participate in the shield pool — their durability is managed in combat.
            if (shield.ItemData != null && (shield.ItemData.IsVest || shield.ItemData.IsSuicideVest))
                return;

            UserRpStats stats = habbo?.GetRpStats();
            if (stats == null)
                return;

            shield.DurabilityLeft = Math.Max(0, stats.Shield);
            if (stats.Shield <= 0) {
                shield.Equipped = false;
                Remove(shield);
                RpItemClothingService.Refresh(habbo);
            }
            MarkDirty();
        }

        private static bool IsBlockedByDeath(Habbo habbo, RpItemData data)
        {
            if (data == null || data.UsableWhileDead || habbo?.GetRpStats()?.IsDead != true)
                return false;

            habbo.GetClient()?.SendWhisper("You're dead — you can't use that.", 1);
            return true;
        }

        private static bool IsBlockedByCuffs(Habbo habbo, RpItemData data)
        {
            if (data == null || data.UsableWhileCuffed || data.Has("handpick"))
                return false;

            if (habbo == null || !PlusEnvironment.GetPoliceManager().IsCuffed(habbo.Id))
                return false;

            habbo.GetClient()?.SendWhisper($"You can't use {data.Name} while cuffed.", 1);
            return true;
        }

        public int DistinctStackCount() =>
            _items.Values.Where(x => !x.Equipped).Select(x => x.ItemId).Distinct().Count();

        public bool Use(int userItemId, Habbo habbo)
        {
            if (habbo == null)
                return false;

            UserRpItem userItem = GetItem(userItemId);
            if (userItem == null)
                return false;

            RpItemData data = userItem.ItemData;
            if (data == null)
                return false;

            UserRpStats stats = habbo.GetRpStats();
            Room room = habbo.CurrentRoom;
            RoomUser roomUser = room?.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);

            // Inert items do nothing at all. Without this they fall through to the consumable
            // path below, which burns a use and destroys the item for no effect.
            if (data.IsNone) {
                habbo.GetClient()?.SendWhisper($"{data.Name} isn't something you can use.", 1);
                return false;
            }

            // Death gate: everything is off limits while dead unless the item opts in with
            // "zombie": 1. Healing yourself up from the grave left health, IsDead and the
            // hospital timer disagreeing with each other.
            if (IsBlockedByDeath(habbo, data))
                return false;

            // Generic passive gate: any item flagged with the "passive" attribute can't be used
            // while the owner is in passive mode (aggressive items — suicide vest, etc.).
            if (data.BlockedWhilePassive && stats != null && stats.PassiveMode) {
                habbo.GetClient()?.SendWhisper($"You can't use {data.Name} while passive!", 1);
                return false;
            }

            if (IsBlockedByCuffs(habbo, data))
                return false;

            // Lockpick: attempt to break free of cuffs instead of the usual consumable effects.
            if (data.Has("handpick"))
                return TryHandpick(habbo, userItem, data);

            // Suicide vest: detonate for area damage when the equipped vest is used.
            if (data.IsSuicideVest)
                return DetonateSuicideVest(habbo, userItem);

            // Cooldowns
            if (data.CooldownMs > 0 &&
                !PlusEnvironment.GetRpCooldownManager().TryConsume(habbo.GetClient(), RpCooldownKind.RpItem,
                    $"You need to wait %seconds% second(s) before using {data.Name} again.",
                    data.CooldownMs / 1000.0, data.Id))
                return false;

            // Consume shout ("text;bubbleId") fires the moment the item is used.
            EmitConsumeBubble(habbo, roomUser, data);

            // give fine shyte
            if (stats != null)
                ApplyStatEffects(habbo, stats, data, roomUser);

            // an item that drops you to 0 health kills you (no attacker)
            bool died = stats != null && stats.Health <= 0 && !stats.IsDead && roomUser != null;
            if (died) {
                LiveFeedService.LiveFeed("💀 " + LiveFeedService.Name(habbo.Username, "green") + " died of item over usage.", "red");
                UserRpStats.Kill(habbo, roomUser);
            }

            // effect or handitem, if 1 exists (skipped if the item just killed us)
            if (!died && roomUser != null && (data.HanditemData != 0 || data.EffectData != 0))
                roomUser.ApplyTimedItemConsumption(TimeSpan.FromMilliseconds(data.ConsumptionDurationMs), data.EffectData, data.HanditemData);

            // uses
            ConsumeUse(userItem);

            habbo.TrySendUserStatsUpdate(true);

            // Live-refresh an open inventory panel (use count / durability / removal).
            if (habbo.GetClient() != null)
                Plus.Communication.Packets.Incoming.WebOverlayCallbackEvent.RefreshInventory(habbo.GetClient());

            return true;
        }

        private bool DetonateSuicideVest(Habbo habbo, UserRpItem userItem)
        {
            if (habbo.GetRpStats().IsDead) {
                habbo.GetClient()?.SendWhisper("A dead person cannot die twice.", 1);
                return false;
            }

            if (!userItem.Equipped) {
                habbo.GetClient()?.SendWhisper("You need to equip the suicide vest first.", 1);
                return false;
            }

            // Passive gate is enforced generically in Use() via the "passive" attribute.
            UserRpStats stats = habbo.GetRpStats();
            RpItemData data = userItem.ItemData;
            RoomUser roomUser = habbo.CurrentRoom?.GetRoomUserManager()?.GetRoomUserByHabbo(habbo.Id);

            // Show the vest's activation effect/handitem the instant it's triggered.
            if (roomUser != null && (data.HanditemData != 0 || data.EffectData != 0))
                roomUser.ApplyTimedItemConsumption(TimeSpan.FromMilliseconds(data.ConsumptionDurationMs), data.EffectData, data.HanditemData);

            EmitConsumeBubble(habbo, roomUser, data);

            // The bomber is dropped to 0 HP; ApplyExplosion resolves the self knockout + blast.
            stats?.Health = 0;

            PlusEnvironment.GetCombatManager().ApplyExplosion(habbo, data.ExplosionMin, data.ExplosionMax);

            // The vest is destroyed once used (single use, no durability).
            Remove(userItem);
            // Restore the wearer's real figure now the clothing-bearing vest is gone.
            RpItemClothingService.Refresh(habbo);
            habbo.TrySendUserStatsUpdate(true);
            return true;
        }

        public int AbsorbWithVest(Habbo habbo, int incomingDamage, bool isFistAttack)
        {
            if (isFistAttack || incomingDamage <= 0)
                return incomingDamage;

            UserRpItem vest = GetEquippedVest();
            if (vest?.ItemData == null)
                return incomingDamage;

            int current = vest.EffectiveDurability;
            if (current <= 0)
                return incomingDamage;

            // Every qualifying hit costs the vest one durability, even if it fails to reduce.
            vest.DurabilityLeft = current - 1;

            int reduced = incomingDamage;
            bool failed = Random.Shared.Next(0, 100) < vest.ItemData.VestFailChance;
            if (!failed) {
                int reduction = Random.Shared.Next(vest.ItemData.VestReductionMin, vest.ItemData.VestReductionMax + 1);
                reduced = Math.Max(0, incomingDamage - reduction);
            }

            if (vest.DurabilityLeft <= 0) {
                vest.Equipped = false;
                Remove(vest);
                RpItemClothingService.Refresh(habbo);
                habbo?.GetClient()?.SendWhisper("Your vest has been destroyed.", 1);
            }

            MarkDirty();
            habbo?.TrySendUserStatsUpdate(true);

            if (habbo?.GetClient() != null)
                WebOverlayCallbackEvent.RefreshInventory(habbo.GetClient());

            return reduced;
        }

        private bool TryHandpick(Habbo habbo, UserRpItem userItem, RpItemData data)
        {
            var police = PlusEnvironment.GetPoliceManager();

            if (!police.IsCuffed(habbo.Id)) {
                habbo.GetClient()?.SendWhisper("You're not cuffed.", 1);
                return false;
            }

            // Can't pick the lock unless you're free to move (not frozen: fresh cuff or escort).
            if (police.IsFrozen(habbo.Id)) {
                habbo.GetClient()?.SendWhisper("You can't pick your cuffs while you're being taken in.", 1);
                return false;
            }

            if (PlusEnvironment.GetJailManager().IsJailed(habbo.Id)) {
                habbo.GetClient()?.SendWhisper("You can't pick your cuffs while in jail.", 1);
                return false;
            }

            int chance = data.GetInt("handpick", 0);
            ConsumeUse(userItem);

            if (Random.Shared.Next(1, 101) <= chance) {
                police.BreakFree(habbo);
                habbo.GetClient()?.SendWhisper("You picked your cuffs and slipped free!", 1);
            } else {
                habbo.GetClient()?.SendWhisper("The lockpick slipped — you're still cuffed.", 1);
            }

            habbo.TrySendUserStatsUpdate(true);
            return true;
        }

        private static void EmitConsumeBubble(Habbo habbo, RoomUser roomUser, RpItemData data)
        {
            if (data == null || !data.HasBubble || roomUser == null || habbo?.CurrentRoom == null)
                return;

            habbo.CurrentRoom.SendPacket(new ShoutComposer(roomUser.VirtualId, data.BubbleText, 0, data.BubbleId, isRpAction: true));
        }

        private static void ApplyStatEffects(Habbo habbo, UserRpStats stats, RpItemData data, RoomUser roomUser)
        {
            if (data.Has("hp")) {
                // slow_heal items drip the HP over time (see SlowHealManager); everything else is
                // the classic instant top-up.
                if (data.IsSlowHeal)
                    PlusEnvironment.GetSlowHealManager()?.Begin(habbo, data.GetInt("hp"));
                else
                    stats.Health = Math.Clamp(stats.Health + data.GetInt("hp"), 0, UserRpStats.GetMaxHealth(habbo));
            }

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

            if (data.Has("vip"))
                habbo.GrantVip(data.GetInt("vip"));
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