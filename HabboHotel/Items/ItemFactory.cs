using System;
using System.Collections.Generic;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Items
{
    public static class ItemFactory
    {
        public static Item CreateSingleItemNullable(ItemData data, Habbo habbo, string extraData, string displayFlags, int groupId = 0, int limitedNumber = 0, int limitedStack = 0)
        {
            if (data == null) throw new InvalidOperationException("Data cannot be null.");

            Item item = new(0, 0, data.Id, extraData, 0, 0, 0, 0, habbo.Id, groupId, limitedNumber, limitedStack, "");

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `items` (base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data,`limited_number`,`limited_stack`) VALUES (@did,@uid,@rid,@x,@y,@z,@wall_pos,@rot,@extra_data, @limited_number, @limited_stack)");
                dbClient.AddParameter("did", data.Id);
                dbClient.AddParameter("uid", habbo.Id);
                dbClient.AddParameter("rid", 0);
                dbClient.AddParameter("x", 0);
                dbClient.AddParameter("y", 0);
                dbClient.AddParameter("z", 0);
                dbClient.AddParameter("wall_pos", "");
                dbClient.AddParameter("rot", 0);
                dbClient.AddParameter("extra_data", extraData);
                dbClient.AddParameter("limited_number", limitedNumber);
                dbClient.AddParameter("limited_stack", limitedStack);
                item.Id = Convert.ToInt32(dbClient.InsertQuery());

                if (groupId > 0) {
                    dbClient.SetQuery("INSERT INTO `items_groups` (`id`, `group_id`) VALUES (@id, @gid)");
                    dbClient.AddParameter("id", item.Id);
                    dbClient.AddParameter("gid", groupId);
                    dbClient.RunQuery();
                }

                return item;
            }
        }

        public static Item CreateSingleItem(ItemData data, Habbo habbo, string extraData, string displayFlags, int itemId, int limitedNumber = 0, int limitedStack = 0)
        {
            if (data == null) throw new InvalidOperationException("Data cannot be null.");

            Item item = new(itemId, 0, data.Id, extraData, 0, 0, 0, 0, habbo.Id, 0, limitedNumber, limitedStack, "");

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `items` (`id`,base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data,`limited_number`,`limited_stack`) VALUES (@id, @did,@uid,@rid,@x,@y,@z,@wall_pos,@rot,@extra_data, @limited_number, @limited_stack)");
                dbClient.AddParameter("id", itemId);
                dbClient.AddParameter("did", data.Id);
                dbClient.AddParameter("uid", habbo.Id);
                dbClient.AddParameter("rid", 0);
                dbClient.AddParameter("x", 0);
                dbClient.AddParameter("y", 0);
                dbClient.AddParameter("z", 0);
                dbClient.AddParameter("wall_pos", "");
                dbClient.AddParameter("rot", 0);
                dbClient.AddParameter("extra_data", extraData);
                dbClient.AddParameter("limited_number", limitedNumber);
                dbClient.AddParameter("limited_stack", limitedStack);
                dbClient.RunQuery();

                return item;
            }
        }

        public static Item CreateGiftItem(ItemData data, Habbo habbo, string extraData, string displayFlags, int itemId, int limitedNumber = 0, int limitedStack = 0)
        {
            if (data == null) throw new InvalidOperationException("Data cannot be null.");

            Item item = new(itemId, 0, data.Id, extraData, 0, 0, 0, 0, habbo.Id, 0, limitedNumber, limitedStack, "");

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `items` (`id`,base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data,`limited_number`,`limited_stack`) VALUES (@id, @did,@uid,@rid,@x,@y,@z,@wall_pos,@rot,@extra_data, @limited_number, @limited_stack)");
                dbClient.AddParameter("id", itemId);
                dbClient.AddParameter("did", data.Id);
                dbClient.AddParameter("uid", habbo.Id);
                dbClient.AddParameter("rid", 0);
                dbClient.AddParameter("x", 0);
                dbClient.AddParameter("y", 0);
                dbClient.AddParameter("z", 0);
                dbClient.AddParameter("wall_pos", "");
                dbClient.AddParameter("rot", 0);
                dbClient.AddParameter("extra_data", extraData);
                dbClient.AddParameter("limited_number", limitedNumber);
                dbClient.AddParameter("limited_stack", limitedStack);
                dbClient.RunQuery();

                return item;
            }
        }

        public static List<Item> CreateMultipleItems(ItemData data, Habbo habbo, string extraData, int amount, int groupId = 0)
        {
            if (data == null) throw new InvalidOperationException("Data cannot be null.");

            List<Item> items = new();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                for (int i = 0; i < amount; i++)
                {
                    dbClient.SetQuery("INSERT INTO `items` (base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data) VALUES(@did,@uid,@rid,@x,@y,@z,@wallpos,@rot,@flags);");
                    dbClient.AddParameter("did", data.Id);
                    dbClient.AddParameter("uid", habbo.Id);
                    dbClient.AddParameter("rid", 0);
                    dbClient.AddParameter("x", 0);
                    dbClient.AddParameter("y", 0);
                    dbClient.AddParameter("z", 0);
                    dbClient.AddParameter("wallpos", "");
                    dbClient.AddParameter("rot", 0);
                    dbClient.AddParameter("flags", extraData);

                    Item item = new(Convert.ToInt32(dbClient.InsertQuery()), 0, data.Id, extraData, 0, 0, 0, 0, habbo.Id, groupId, 0, 0, "");

                    if (groupId > 0) {
                        dbClient.SetQuery("INSERT INTO `items_groups` (`id`, `group_id`) VALUES (@id, @gid)");
                        dbClient.AddParameter("id", item.Id);
                        dbClient.AddParameter("gid", groupId);
                        dbClient.RunQuery();
                    }

                    items.Add(item);
                }
            }

            return items;
        }

        public static List<Item> CreateTeleporterItems(ItemData data, Habbo habbo, int groupId = 0)
        {
            List<Item> items = new();

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `items` (base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data) VALUES(@did,@uid,@rid,@x,@y,@z,@wallpos,@rot,@flags);");
                dbClient.AddParameter("did", data.Id);
                dbClient.AddParameter("uid", habbo.Id);
                dbClient.AddParameter("rid", 0);
                dbClient.AddParameter("x", 0);
                dbClient.AddParameter("y", 0);
                dbClient.AddParameter("z", 0);
                dbClient.AddParameter("wallpos", "");
                dbClient.AddParameter("rot", 0);
                dbClient.AddParameter("flags", "");

                int item1Id = Convert.ToInt32(dbClient.InsertQuery());

                dbClient.SetQuery("INSERT INTO `items` (base_item,user_id,room_id,x,y,z,wall_pos,rot,extra_data) VALUES(@did,@uid,@rid,@x,@y,@z,@wallpos,@rot,@flags);");
                dbClient.AddParameter("did", data.Id);
                dbClient.AddParameter("uid", habbo.Id);
                dbClient.AddParameter("rid", 0);
                dbClient.AddParameter("x", 0);
                dbClient.AddParameter("y", 0);
                dbClient.AddParameter("z", 0);
                dbClient.AddParameter("wallpos", "");
                dbClient.AddParameter("rot", 0);
                dbClient.AddParameter("flags", item1Id.ToString());

                int item2Id = Convert.ToInt32(dbClient.InsertQuery());

                Item item1 = new(item1Id, 0, data.Id, "", 0, 0, 0, 0, habbo.Id, groupId, 0, 0, "");
                Item item2 = new(item2Id, 0, data.Id, "", 0, 0, 0, 0, habbo.Id, groupId, 0, 0, "");

                dbClient.SetQuery("INSERT INTO `room_items_tele_links` (`tele_one_id`, `tele_two_id`) VALUES (" + item1Id + ", " + item2Id + "), (" + item2Id + ", " + item1Id + ")");
                dbClient.RunQuery();

                items.Add(item1);
                items.Add(item2);
            }

            return items;
        }

        public static void CreateMoodlightData(Item item)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `room_items_moodlight` (`id`, `enabled`, `current_preset`, `preset_one`, `preset_two`, `preset_three`) VALUES (@id, '0', 1, @preset, @preset, @preset);");
                dbClient.AddParameter("id", item.Id);
                dbClient.AddParameter("preset", "#000000,255,0");
                dbClient.RunQuery();
            }
        }

        public static void CreateTonerData(Item item)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `room_items_toner` (`id`, `data1`, `data2`, `data3`, `enabled`) VALUES (@id, 0, 0, 0, '0')");
                dbClient.AddParameter("id", item.Id);
                dbClient.RunQuery();
            }
        }
    }
}