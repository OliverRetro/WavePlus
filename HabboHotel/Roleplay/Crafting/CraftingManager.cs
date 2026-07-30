using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using log4net;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Crafting;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.RpItem;
using Plus.HabboHotel.Roleplay.RpItem.Item;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Roleplay.Crafting
{
    public sealed class CraftingManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CraftingManager));

        private readonly Dictionary<int, CraftingRecipe> _recipesById = [];
        private readonly Dictionary<string, CraftingRecipe> _recipesByName = new(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<int, Dictionary<int, bool>> _groupUnlocks = [];
        private readonly Lock _groupSync = new();

        private readonly ConcurrentDictionary<int, ActiveCraft> _activeCrafts = new();

        public void Init()
        {
            _recipesById.Clear();
            _recipesByName.Clear();
            lock (_groupSync)
                _groupUnlocks.Clear();

            try {
                using WavePlusContext db = PlusEnvironment.GetDbContext();

                foreach (CraftingRecipeEntity row in db.CraftingRecipes.AsNoTracking().Where(r => r.Enabled).ToList()) {
                    CraftingRecipe recipe = new(row.Id, row.Name, row.RewardType, row.RewardId, row.RewardAmount, row.Secret);
                    _recipesById[recipe.Id] = recipe;
                    _recipesByName[recipe.Name] = recipe;
                }

                foreach (CraftingRecipeIngredientEntity row in db.CraftingRecipeIngredients.AsNoTracking().ToList()) {
                    if (_recipesById.TryGetValue(row.RecipeId, out CraftingRecipe recipe))
                        recipe.Ingredients.Add(new CraftingIngredient(row.ItemType, row.ItemId, row.Amount));
                }

                foreach (GroupCraftingRecipeEntity row in db.GroupCraftingRecipes.AsNoTracking().ToList()) {
                    if (!_groupUnlocks.TryGetValue(row.GroupId, out Dictionary<int, bool> set))
                        _groupUnlocks[row.GroupId] = set = new Dictionary<int, bool>();
                    set[row.RecipeId] = row.Revealed;
                }

                Log.Info($"[RP] Loaded {_recipesById.Count} crafting recipes.");
            } catch (Exception e) {
                Log.Error("[RP] Failed to load crafting recipes. Crafting disabled.", e);
            }
        }

        public CraftingRecipe GetById(int id) => _recipesById.TryGetValue(id, out CraftingRecipe r) ? r : null;

        public CraftingRecipe GetByName(string name) =>
            name != null && _recipesByName.TryGetValue(name, out CraftingRecipe r) ? r : null;

        public IEnumerable<CraftingRecipe> AllRecipes => _recipesById.Values;

        public static bool Describe(string itemType, int itemId, out string name, out string iconUrl, out int rarity)
        {
            name = "";
            iconUrl = "";
            rarity = 1;

            switch (itemType) {
                case "rp_item":
                    if (!PlusEnvironment.GetRpItemManager().TryGetItem(itemId, out RpItemData item))
                        return false;
                    name = item.Name ?? "";
                    iconUrl = item.ImageUrl ?? "";
                    rarity = item.Rarity;
                    return true;

                case "rp_weapon":
                    if (!PlusEnvironment.GetWeaponManager().TryGetWeapon(itemId, out Weapon weapon))
                        return false;
                    name = weapon.Name ?? "";
                    iconUrl = weapon.Image ?? "";
                    rarity = weapon.Rarity < 1 ? 1 : weapon.Rarity;
                    return true;

                default:
                    return false;
            }
        }

        public CraftingRecipe MatchByMultiset(Dictionary<(string Type, int Id), int> guess, Func<int, bool> isUnlocked)
        {
            if (guess == null || guess.Count == 0)
                return null;

            foreach (CraftingRecipe recipe in _recipesById.Values) {
                if (!isUnlocked(recipe.Id))
                    continue;

                Dictionary<(string, int), int> need = recipe.AsMultiset();
                if (need.Count != guess.Count)
                    continue;

                bool equal = true;
                foreach (KeyValuePair<(string, int), int> pair in need) {
                    if (!guess.TryGetValue(pair.Key, out int have) || have != pair.Value) {
                        equal = false;
                        break;
                    }
                }

                if (equal)
                    return recipe;
            }

            return null;
        }

        public bool IsGroupUnlocked(int groupId, int recipeId)
        {
            lock (_groupSync)
                return _groupUnlocks.TryGetValue(groupId, out Dictionary<int, bool> set) && set.ContainsKey(recipeId);
        }

        public bool IsGroupRevealed(int groupId, int recipeId)
        {
            lock (_groupSync)
                return _groupUnlocks.TryGetValue(groupId, out Dictionary<int, bool> set) &&
                    set.TryGetValue(recipeId, out bool revealed) && revealed;
        }

        public bool UnlockGroup(int groupId, int recipeId)
        {
            lock (_groupSync) {
                if (!_groupUnlocks.TryGetValue(groupId, out Dictionary<int, bool> set))
                    _groupUnlocks[groupId] = set = new Dictionary<int, bool>();

                if (set.ContainsKey(recipeId))
                    return false;

                set[recipeId] = false;
            }

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            bool exists = db.GroupCraftingRecipes.Any(g => g.GroupId == groupId && g.RecipeId == recipeId);
            if (!exists) {
                db.GroupCraftingRecipes.Add(new GroupCraftingRecipeEntity { GroupId = groupId, RecipeId = recipeId, Revealed = false });
                db.SaveChanges();
            }

            return true;
        }

        public void RevealGroup(int groupId, int recipeId)
        {
            lock (_groupSync) {
                if (!_groupUnlocks.TryGetValue(groupId, out Dictionary<int, bool> set) || !set.ContainsKey(recipeId))
                    return;
                if (set[recipeId])
                    return;
                set[recipeId] = true;
            }

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            db.GroupCraftingRecipes
                .Where(g => g.GroupId == groupId && g.RecipeId == recipeId)
                .ExecuteUpdate(s => s.SetProperty(g => g.Revealed, true));
        }

        public (int State, int Count, CraftingRecipe Exact) EvaluateHint(
            Dictionary<(string Type, int Id), int> guess, Func<int, bool> isUnlocked)
        {
            if (guess == null || guess.Count == 0)
                return (0, 0, null);

            CraftingRecipe exact = MatchByMultiset(guess, isUnlocked);
            if (exact != null)
                return (2, 0, exact);

            int count = 0;
            foreach (CraftingRecipe recipe in _recipesById.Values) {
                if (!isUnlocked(recipe.Id))
                    continue;
                if (IsSubset(guess, recipe.AsMultiset()))
                    count++;
            }

            return count > 0 ? (1, count, null) : (0, 0, null);
        }

        private static bool IsSubset(Dictionary<(string Type, int Id), int> guess, Dictionary<(string Type, int Id), int> recipe)
        {
            foreach (KeyValuePair<(string Type, int Id), int> pair in guess) {
                if (!recipe.TryGetValue(pair.Key, out int have) || have < pair.Value)
                    return false;
            }
            return true;
        }

        public bool IsCrafting(int userId) => _activeCrafts.ContainsKey(userId);

        public void StartCraft(GameClient session, int objectId, Dictionary<(string Type, int Id), int> guess)
        {
            Habbo habbo = session?.GetHabbo();
            if (habbo == null)
                return;

            // Already crafting — ignore (the client is already showing its timer).
            if (_activeCrafts.ContainsKey(habbo.Id))
                return;

            ICraftingSource source = CraftingContext.Resolve(session, objectId);
            if (source == null) {
                session.SendPacket(new CraftingResultComposer(false));
                return;
            }

            CraftingRecipe recipe = MatchByMultiset(guess, source.IsUnlocked);
            if (recipe == null) {
                session.SendPacket(new CraftingResultComposer(false));
                return;
            }

            bool isCorp = source is CorporationCraftingSource;

            if (!isCorp && RpInventory.UsedSlots(habbo, 10) >= 10) {
                session.SendWhisper("Your inventory is full — free up a slot before crafting.", 1);
                session.SendPacket(new CraftingResultComposer(false));
                return;
            }

            Dictionary<(string Type, int Id), int> needed = recipe.AsMultiset();
            foreach (KeyValuePair<(string Type, int Id), int> ingredient in needed) {
                if (source.CountOwned(ingredient.Key.Type, ingredient.Key.Id) < ingredient.Value) {
                    session.SendPacket(new CraftingResultComposer(false));
                    return;
                }
            }

            foreach (KeyValuePair<(string Type, int Id), int> ingredient in needed)
                source.Consume(ingredient.Key.Type, ingredient.Key.Id, ingredient.Value);
            source.CommitAndRefresh();

            Room room = habbo.CurrentRoom;
            int seconds = RpInteractionTimer.GetSeconds(habbo);
            ActiveCraft craft = new()
            {
                UserId = habbo.Id,
                RoomId = room?.Id ?? 0,
                RecipeId = recipe.Id,
                IsCorp = isCorp,
                GroupId = isCorp ? (room?.Group?.Id ?? 0) : 0,
                FinishUnix = PlusEnvironment.GetUnixTimestampPrecise() + seconds + RpInteractionTimer.CompletionGraceSeconds
            };

            if (!_activeCrafts.TryAdd(habbo.Id, craft)) {
                // Lost a race — put the ingredients straight back.
                ReturnIngredients(source, recipe);
                source.CommitAndRefresh();
                session.SendPacket(new CraftingResultComposer(false));
                return;
            }

            Describe(recipe.RewardType, recipe.RewardId, out string rewardName, out _, out _);
            Shout(habbo, $"*starts crafting {rewardName}*");
        }

        public void CancelCraft(Habbo habbo, bool notify = true)
        {
            if (habbo == null || !_activeCrafts.TryRemove(habbo.Id, out ActiveCraft craft))
                return;

            CraftingRecipe recipe = GetById(craft.RecipeId);
            if (recipe == null)
                return;

            ICraftingSource source = BuildSource(craft, habbo);
            ReturnIngredients(source, recipe);
            source.CommitAndRefresh();

            if (notify) {
                Shout(habbo, "*canceled crafting*");
                habbo.GetClient()?.SendPacket(new CraftingResultComposer(false));
            }
        }

        public void OnCycle()
        {
            if (_activeCrafts.IsEmpty)
                return;

            double now = PlusEnvironment.GetUnixTimestampPrecise();
            foreach (KeyValuePair<int, ActiveCraft> entry in _activeCrafts) {
                if (entry.Value.FinishUnix > now)
                    continue;

                if (_activeCrafts.TryRemove(entry.Key, out ActiveCraft craft))
                    CompleteCraft(craft);
            }
        }

        private void CompleteCraft(ActiveCraft craft)
        {
            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(craft.UserId);
            Habbo habbo = client?.GetHabbo();
            CraftingRecipe recipe = GetById(craft.RecipeId);
            if (habbo == null || recipe == null)
                return;

            ICraftingSource source = BuildSource(craft, habbo);

            Room room = habbo.CurrentRoom;
            if (room == null || room.Id != craft.RoomId) {
                ReturnIngredients(source, recipe);
                source.CommitAndRefresh();
                client.SendPacket(new CraftingResultComposer(false));
                return;
            }

            source.Grant(recipe.RewardType, recipe.RewardId, recipe.RewardAmount);
            source.CommitAndRefresh();

            if (recipe.Secret)
                source.Reveal(recipe.Id);

            Describe(recipe.RewardType, recipe.RewardId, out string name, out string icon, out _);
            Shout(habbo, $"*crafted {name}*");
            client.SendPacket(new CraftingResultComposer(true, recipe.Name, $"{recipe.RewardType}:{recipe.RewardId}", name, icon));
        }

        private static ICraftingSource BuildSource(ActiveCraft craft, Habbo habbo) =>
            craft.IsCorp ? new CorporationCraftingSource(craft.RoomId, craft.GroupId) : new PersonalCraftingSource(habbo);

        private static void ReturnIngredients(ICraftingSource source, CraftingRecipe recipe)
        {
            foreach (KeyValuePair<(string Type, int Id), int> ingredient in recipe.AsMultiset())
                source.Grant(ingredient.Key.Type, ingredient.Key.Id, ingredient.Value);
        }

        private static void Shout(Habbo habbo, string text)
        {
            Room room = habbo?.CurrentRoom;
            RoomUser roomUser = room?.GetRoomUserManager()?.GetRoomUserByHabbo(habbo.Id);
            if (roomUser != null)
                room.SendPacket(new ShoutComposer(roomUser.VirtualId, text, 0, 4, isRpAction: true));
        }

        private sealed class ActiveCraft
        {
            public int UserId;
            public int RoomId;
            public int RecipeId;
            public bool IsCorp;
            public int GroupId;
            public double FinishUnix;
        }
    }
}