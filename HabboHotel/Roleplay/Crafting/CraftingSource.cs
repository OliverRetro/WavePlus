using System.Linq;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.RpItem.Item;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Roleplay.Stock;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Roleplay.Crafting
{
    public interface ICraftingSource
    {
        bool IsUnlocked(int recipeId);
        bool IsVisible(CraftingRecipe recipe);

        int CountOwned(string itemType, int itemId);

        bool Consume(string itemType, int itemId, int amount);

        bool Grant(string itemType, int itemId, int amount);

        void Reveal(int recipeId);

        void CommitAndRefresh();
    }

    public sealed class PersonalCraftingSource : ICraftingSource
    {
        private readonly Habbo _habbo;
        private readonly UserCraftingRecipes _recipes;

        public PersonalCraftingSource(Habbo habbo)
        {
            _habbo = habbo;
            _recipes = habbo.GetCraftingRecipes();
        }

        public bool IsUnlocked(int recipeId) => _recipes != null && _recipes.IsUnlocked(recipeId);

        public bool IsVisible(CraftingRecipe recipe) => _recipes != null && _recipes.IsVisible(recipe);

        public int CountOwned(string itemType, int itemId)
        {
            switch (itemType) {
                case "rp_item":
                    return _habbo.GetRpItems()?.GetItemsByItemId(itemId).Count(x => !x.Equipped) ?? 0;
                case "rp_weapon":
                    UserRpWeapons weapons = _habbo.GetRpWeapons();
                    if (weapons == null)
                        return 0;
                    int active = weapons.ActiveWeaponId;
                    return weapons.GetWeaponsByWeaponId(itemId).Count(w => !w.IsDefault && w.Id != active);
                default:
                    return 0;
            }
        }

        public bool Consume(string itemType, int itemId, int amount)
        {
            if (CountOwned(itemType, itemId) < amount)
                return false;

            switch (itemType) {
                case "rp_item": {
                        UserRpItems items = _habbo.GetRpItems();
                        foreach (UserRpItem item in items.GetItemsByItemId(itemId).Where(x => !x.Equipped).Take(amount).ToList())
                            items.Remove(item);
                        return true;
                    }
                case "rp_weapon": {
                        UserRpWeapons weapons = _habbo.GetRpWeapons();
                        int active = weapons.ActiveWeaponId;
                        foreach (UserWeapon weapon in weapons.GetWeaponsByWeaponId(itemId).Where(w => !w.IsDefault && w.Id != active).Take(amount).ToList())
                            weapons.RemoveWeapon(weapon.Id);
                        return true;
                    }
                default:
                    return false;
            }
        }

        public bool Grant(string itemType, int itemId, int amount)
        {
            switch (itemType) {
                case "rp_item": {
                        UserRpItems items = _habbo.GetRpItems();
                        if (items == null)
                            return false;
                        for (int i = 0; i < amount; i++)
                            if (items.AddItem(itemId) == null)
                                return i > 0;
                        return true;
                    }
                case "rp_weapon": {
                        UserRpWeapons weapons = _habbo.GetRpWeapons();
                        if (weapons == null)
                            return false;
                        for (int i = 0; i < amount; i++)
                            if (weapons.AddWeapon(itemId) == null)
                                return i > 0;
                        return true;
                    }
                default:
                    return false;
            }
        }

        public void Reveal(int recipeId) => _recipes?.Reveal(recipeId);

        public void CommitAndRefresh()
        {
            _habbo.SaveRpItems();
            _habbo.SaveRpWeapons();

            GameClient client = _habbo.GetClient();
            if (client != null)
                WebOverlayCallbackEvent.RefreshInventory(client);
        }
    }

    public sealed class CorporationCraftingSource : ICraftingSource
    {
        private readonly int _roomId;
        private readonly int _groupId;
        private readonly CraftingManager _manager;

        public CorporationCraftingSource(int roomId, int groupId)
        {
            _roomId = roomId;
            _groupId = groupId;
            _manager = PlusEnvironment.GetCraftingManager();
        }

        private static string ToStockType(string itemType) =>
            itemType == "rp_weapon" ? StockType.RpWeapon : StockType.RpItem;

        public bool IsUnlocked(int recipeId) => _manager.IsGroupUnlocked(_groupId, recipeId);

        public bool IsVisible(CraftingRecipe recipe) =>
            recipe != null && _manager.IsGroupUnlocked(_groupId, recipe.Id) &&
            (!recipe.Secret || _manager.IsGroupRevealed(_groupId, recipe.Id));

        public int CountOwned(string itemType, int itemId) =>
            PlusEnvironment.GetRoomStockManager().GetStock(_roomId, ToStockType(itemType), itemId, RoomStockManager.RoomScope);

        public bool Consume(string itemType, int itemId, int amount) =>
            PlusEnvironment.GetRoomStockManager().TryConsume(_roomId, ToStockType(itemType), itemId, amount, RoomStockManager.RoomScope);

        public bool Grant(string itemType, int itemId, int amount)
        {
            PlusEnvironment.GetRoomStockManager().AddStock(_roomId, ToStockType(itemType), itemId, amount, RoomStockManager.RoomScope);
            return true;
        }

        public void Reveal(int recipeId) => _manager.RevealGroup(_groupId, recipeId);

        public void CommitAndRefresh()
        {
            // RoomStockManager persists its own dirty entries on its save cycle; nothing to flush here.
        }
    }
}