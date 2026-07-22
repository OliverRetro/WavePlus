using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;
using Plus.Database.EF;
using Microsoft.EntityFrameworkCore;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Users.Inventory.Bots;
using Plus.HabboHotel.Users.Inventory.Pets;

namespace Plus.HabboHotel.Users.Inventory
{
    public class InventoryComponent
    {
        private readonly int _userId;
        private GameClient _client;

        private readonly ConcurrentDictionary<int, Bot> _botItems;
        private readonly ConcurrentDictionary<int, Pet> _petsItems;
        private readonly ConcurrentDictionary<int, Item> _floorItems;
        private readonly ConcurrentDictionary<int, Item> _wallItems;

        public InventoryComponent(int userId, GameClient client)
        {
            _client = client;
            _userId = userId;

            _floorItems = new ConcurrentDictionary<int, Item>();
            _wallItems = new ConcurrentDictionary<int, Item>();
            _petsItems = new ConcurrentDictionary<int, Pet>();
            _botItems = new ConcurrentDictionary<int, Bot>();

            Init();
        }

        public void Init()
        {
            if (!_floorItems.IsEmpty)
                _floorItems.Clear();
            if (!_wallItems.IsEmpty)
                _wallItems.Clear();
            if (!_petsItems.IsEmpty)
                _petsItems.Clear();
            if (!_botItems.IsEmpty)
                _botItems.Clear();

            List<Item> items = ItemLoader.GetItemsForUser(_userId);
            foreach (Item item in items.ToList()) {
                if (item.IsFloorItem) {
                    if (!_floorItems.TryAdd(item.Id, item))
                        continue;
                } else if (item.IsWallItem) {
                    if (!_wallItems.TryAdd(item.Id, item))
                        continue;
                } else
                    continue;
            }

            List<Pet> pets = PetLoader.GetPetsForUser(Convert.ToInt32(_userId));
            foreach (Pet pet in pets) {
                if (!_petsItems.TryAdd(pet.PetId, pet)) {
                    Console.WriteLine("Error whilst loading pet x1: " + pet.PetId);
                }
            }

            List<Bot> bots = BotLoader.GetBotsForUser(Convert.ToInt32(_userId));
            foreach (Bot bot in bots) {
                if (!_botItems.TryAdd(bot.Id, bot)) {
                    Console.WriteLine("Error whilst loading bot x1: " + bot.Id);
                }
            }
        }

        public void ClearItems()
        {
            UpdateItems(true);

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Items.Where(i => i.RoomId == 0 && i.UserId == _userId).ExecuteDelete();
            }

            _floorItems.Clear();
            _wallItems.Clear();

            _client?.SendPacket(new FurniListUpdateComposer());
        }

        public void SetIdleState()
        {
            _botItems?.Clear();

            _petsItems?.Clear();

            _floorItems?.Clear();

            _wallItems?.Clear();

            _client = null;
        }

        public void UpdateItems(bool fromDatabase)
        {
            if (fromDatabase)
                Init();

            _client?.SendPacket(new FurniListUpdateComposer());
        }

        public Item GetItem(int id)
        {
            if (_floorItems.TryGetValue(id, out Item value))
                return value;

            return _wallItems.TryGetValue(id, out Item value1) ? value1 : null;
        }

        public IEnumerable<Item> GetItems => _floorItems.Values.Concat(_wallItems.Values);

        public Item AddNewItem(int id, int baseItem, string extraData, int group, bool toInsert, bool fromRoom, int limitedNumber, int limitedStack)
        {
            if (toInsert) {
                if (fromRoom) {
                    uint fromRoomId = (uint)id;
                    int uid = _userId;
                    using WavePlusContext db = PlusEnvironment.GetDbContext();
                    db.Items.Where(i => i.Id == fromRoomId).ExecuteUpdate(s => s
                        .SetProperty(i => i.RoomId, (uint)0)
                        .SetProperty(i => i.UserId, uid));
                } else {
                    using WavePlusContext db = PlusEnvironment.GetDbContext();

                    if (id > 0) {
                        db.Database.ExecuteSqlInterpolated($"INSERT INTO `items` (`id`,`base_item`, `user_id`, `limited_number`, `limited_stack`) VALUES ({id}, {baseItem}, {_userId}, {limitedNumber}, {limitedStack})");
                    } else {
                        var row = new Database.EF.Entities.ItemEntity { BaseItem = (uint)baseItem, UserId = _userId, LimitedNumber = limitedNumber, LimitedStack = limitedStack };
                        db.Items.Add(row);
                        db.SaveChanges();
                        id = (int)row.Id;
                    }

                    SendNewItems(Convert.ToInt32(id));

                    if (group > 0)
                        db.Database.ExecuteSqlInterpolated($"INSERT INTO `items_groups` VALUES ({id}, {group})");

                    if (!string.IsNullOrEmpty(extraData)) {
                        uint newItemId = (uint)id;
                        string ed = extraData;
                        db.Items.Where(i => i.Id == newItemId).ExecuteUpdate(s => s.SetProperty(i => i.ExtraData, ed));
                    }
                }
            }

            Item itemToAdd = new(id, 0, baseItem, extraData, 0, 0, 0, 0, _userId, group, limitedNumber, limitedStack, string.Empty);

            if (UserHoldsItem(id))
                RemoveItem(id);

            if (itemToAdd.IsWallItem)
                _wallItems.TryAdd(itemToAdd.Id, itemToAdd);
            else
                _floorItems.TryAdd(itemToAdd.Id, itemToAdd);
            return itemToAdd;
        }

        private bool UserHoldsItem(int itemId)
        {
            if (_floorItems.ContainsKey(itemId))
                return true;
            if (_wallItems.ContainsKey(itemId))
                return true;
            return false;
        }

        public void RemoveItem(int id)
        {
            if (GetClient() == null)
                return;

            if (GetClient().GetHabbo() == null || GetClient().GetHabbo().GetInventoryComponent() == null)
                GetClient().Disconnect();

            if (_floorItems.ContainsKey(id)) {
                _floorItems.TryRemove(id, out Item _);
            }

            if (_wallItems.ContainsKey(id)) {
                _wallItems.TryRemove(id, out Item _);
            }

            GetClient().SendPacket(new FurniListRemoveComposer(id));
        }

        private GameClient GetClient()
        {
            return PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(_userId);
        }

        public void SendNewItems(int id)
        {
            _client.SendPacket(new FurniListNotificationComposer(id, 1));
        }

        #region Pet Handling

        public ICollection<Pet> GetPets()
        {
            return _petsItems.Values;
        }

        public bool TryAddPet(Pet pet)
        {
            return _petsItems.TryAdd(pet.PetId, pet);
        }

        public bool TryRemovePet(int petId, out Pet petItem)
        {
            if (_petsItems.ContainsKey(petId))
                return _petsItems.TryRemove(petId, out petItem);

            petItem = null;
            return false;
        }

        public bool TryGetPet(int petId, out Pet pet)
        {
            if (_petsItems.ContainsKey(petId))
                return _petsItems.TryGetValue(petId, out pet);
            pet = null;
            return false;
        }

        #endregion

        #region Bot Handling

        public ICollection<Bot> GetBots()
        {
            return _botItems.Values;
        }

        public bool TryAddBot(Bot bot)
        {
            return _botItems.TryAdd(bot.Id, bot);
        }

        public bool TryRemoveBot(int botId, out Bot bot)
        {
            if (_botItems.ContainsKey(botId))
                return _botItems.TryRemove(botId, out bot);
            bot = null;
            return false;
        }

        public bool TryGetBot(int botId, out Bot bot)
        {
            if (_botItems.ContainsKey(botId))
                return _botItems.TryGetValue(botId, out bot);
            bot = null;
            return false;
        }

        #endregion

        public bool TryAddItem(Item item)
        {
            if (item.Data.Type.ToString().Equals("s", StringComparison.CurrentCultureIgnoreCase)) // ItemType.FLOOR)
            {
                return _floorItems.TryAdd(item.Id, item);
            }

            if (item.Data.Type.ToString().Equals("i", StringComparison.CurrentCultureIgnoreCase)) //ItemType.WALL)
            {
                return _wallItems.TryAdd(item.Id, item);
            }

            throw new InvalidOperationException("Item did not match neither floor or wall item");
        }

        public bool TryAddFloorItem(int itemId, Item item)
        {
            return _floorItems.TryAdd(itemId, item);
        }

        public bool TryAddWallItem(int itemId, Item item)
        {
            return _floorItems.TryAdd(itemId, item);
        }

        public ICollection<Item> GetFloorItems()
        {
            return _floorItems.Values;
        }

        public ICollection<Item> GetWallItems()
        {
            return _wallItems.Values;
        }

        public IEnumerable<Item> GetWallAndFloor => _floorItems.Values.Concat(_wallItems.Values);
    }
}