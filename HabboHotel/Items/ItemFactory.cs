using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Items
{
    public static class ItemFactory
    {
        public static Item CreateSingleItemNullable(ItemData data, Habbo habbo, string extraData, string displayFlags, int groupId = 0, int limitedNumber = 0, int limitedStack = 0)
        {
            if (data == null) throw new InvalidOperationException("Data cannot be null.");

            Item item = new(0, 0, data.Id, extraData, 0, 0, 0, 0, habbo.Id, groupId, limitedNumber, limitedStack, "");

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            var row = new Database.EF.Entities.ItemEntity
            {
                BaseItem = (uint)data.Id,
                UserId = habbo.Id,
                RoomId = 0,
                X = 0,
                Y = 0,
                Z = 0,
                Rot = 0,
                WallPos = "",
                ExtraData = extraData,
                LimitedNumber = limitedNumber,
                LimitedStack = limitedStack
            };
            db.Items.Add(row);
            db.SaveChanges();
            item.Id = (int)row.Id;

            // items_groups.id is (mis)used as the furni id, so it must be inserted explicitly.
            if (groupId > 0)
                db.Database.ExecuteSqlInterpolated($"INSERT INTO `items_groups` (`id`, `group_id`) VALUES ({item.Id}, {groupId})");

            return item;
        }

        public static Item CreateSingleItem(ItemData data, Habbo habbo, string extraData, string displayFlags, int itemId, int limitedNumber = 0, int limitedStack = 0)
        {
            if (data == null) throw new InvalidOperationException("Data cannot be null.");

            Item item = new(itemId, 0, data.Id, extraData, 0, 0, 0, 0, habbo.Id, 0, limitedNumber, limitedStack, "");

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            // Explicit id insert into an auto_increment column — ExecuteSqlInterpolated preserves it.
            db.Database.ExecuteSqlInterpolated($"INSERT INTO `items` (`id`,base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data,`limited_number`,`limited_stack`) VALUES ({itemId},{data.Id},{habbo.Id},0,0,0,0,'',0,{extraData},{limitedNumber},{limitedStack})");

            return item;
        }

        public static Item CreateGiftItem(ItemData data, Habbo habbo, string extraData, string displayFlags, int itemId, int limitedNumber = 0, int limitedStack = 0)
        {
            if (data == null) throw new InvalidOperationException("Data cannot be null.");

            Item item = new(itemId, 0, data.Id, extraData, 0, 0, 0, 0, habbo.Id, 0, limitedNumber, limitedStack, "");

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            db.Database.ExecuteSqlInterpolated($"INSERT INTO `items` (`id`,base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data,`limited_number`,`limited_stack`) VALUES ({itemId},{data.Id},{habbo.Id},0,0,0,0,'',0,{extraData},{limitedNumber},{limitedStack})");

            return item;
        }

        public static List<Item> CreateMultipleItems(ItemData data, Habbo habbo, string extraData, int amount, int groupId = 0)
        {
            if (data == null) throw new InvalidOperationException("Data cannot be null.");

            List<Item> items = new();

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            var rows = new List<Database.EF.Entities.ItemEntity>();
            for (int i = 0; i < amount; i++) {
                var row = new Database.EF.Entities.ItemEntity
                {
                    BaseItem = (uint)data.Id,
                    UserId = habbo.Id,
                    RoomId = 0,
                    X = 0,
                    Y = 0,
                    Z = 0,
                    Rot = 0,
                    WallPos = "",
                    ExtraData = extraData
                };
                db.Items.Add(row);
                rows.Add(row);
            }
            db.SaveChanges();

            foreach (var row in rows) {
                Item item = new((int)row.Id, 0, data.Id, extraData, 0, 0, 0, 0, habbo.Id, groupId, 0, 0, "");

                if (groupId > 0)
                    db.Database.ExecuteSqlInterpolated($"INSERT INTO `items_groups` (`id`, `group_id`) VALUES ({item.Id}, {groupId})");

                items.Add(item);
            }

            return items;
        }

        public static List<Item> CreateTeleporterItems(ItemData data, Habbo habbo, int groupId = 0)
        {
            List<Item> items = new();

            using WavePlusContext db = PlusEnvironment.GetDbContext();

            var i1 = new Database.EF.Entities.ItemEntity { BaseItem = (uint)data.Id, UserId = habbo.Id, RoomId = 0, X = 0, Y = 0, Z = 0, Rot = 0, WallPos = "", ExtraData = "" };
            db.Items.Add(i1);
            db.SaveChanges();
            int item1Id = (int)i1.Id;

            var i2 = new Database.EF.Entities.ItemEntity { BaseItem = (uint)data.Id, UserId = habbo.Id, RoomId = 0, X = 0, Y = 0, Z = 0, Rot = 0, WallPos = "", ExtraData = item1Id.ToString() };
            db.Items.Add(i2);
            db.SaveChanges();
            int item2Id = (int)i2.Id;

            Item item1 = new(item1Id, 0, data.Id, "", 0, 0, 0, 0, habbo.Id, groupId, 0, 0, "");
            Item item2 = new(item2Id, 0, data.Id, "", 0, 0, 0, 0, habbo.Id, groupId, 0, 0, "");

            db.RoomItemsTeleLinks.AddRange(
                new Database.EF.Entities.RoomItemsTeleLinkEntity { TeleOneId = (uint)item1Id, TeleTwoId = (uint)item2Id },
                new Database.EF.Entities.RoomItemsTeleLinkEntity { TeleOneId = (uint)item2Id, TeleTwoId = (uint)item1Id });
            db.SaveChanges();

            items.Add(item1);
            items.Add(item2);

            return items;
        }

        public static void CreateMoodlightData(Item item)
        {
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            string preset = "#000000,255,0";
            db.Database.ExecuteSqlInterpolated($"INSERT INTO `room_items_moodlight` (`id`, `enabled`, `current_preset`, `preset_one`, `preset_two`, `preset_three`) VALUES ({item.Id}, '0', 1, {preset}, {preset}, {preset})");
        }

        public static void CreateTonerData(Item item)
        {
            using WavePlusContext db = PlusEnvironment.GetDbContext();
            db.Database.ExecuteSqlInterpolated($"INSERT INTO `room_items_toner` (`id`, `data1`, `data2`, `data3`, `enabled`) VALUES ({item.Id}, 0, 0, 0, '0')");
        }
    }
}