using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Achievements;
using Plus.HabboHotel.Users.Authenticator;
using Plus.HabboHotel.Users.Badges;
using Plus.HabboHotel.Users.Messenger;
using Plus.HabboHotel.Users.Relationships;

namespace Plus.HabboHotel.Users.UserData
{
    public static class UserDataFactory
    {
        public static UserData GetUserData(string sessionTicket, out byte errorCode)
        {
            int userId;
            DataRow dUserInfo = null;
            DataTable dAchievements = null;
            DataTable dFavouriteRooms = null;
            DataTable dBadges = null;
            DataTable dFriends = null;
            DataTable dRequests = null;
            DataTable dQuests = null;
            DataTable dRelations = null;
            DataRow userInfo = null;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`username`,`rank`,`motto`,`look`,`gender`,`last_online`,`credits`,`activity_points`,`home_room`,`home_room_data`,`block_newfriends`,`hide_online`,`hide_inroom`,`vip`,`account_created`,`vip_points`,`machine_id`,`volume`,`chat_preference`,`focus_preference`, `pets_muted`,`bots_muted`,`advertising_report_blocked`,`last_change`,`gotw_points`,`ignore_invites`,`time_muted`,`allow_gifts`,`friend_bar_state`,`disable_forced_effects`,`allow_mimic`,`rank_vip`,`bubble` FROM `users` WHERE `auth_ticket` = @sso LIMIT 1");
                dbClient.AddParameter("sso", sessionTicket);
                dUserInfo = dbClient.GetRow();

                if (dUserInfo == null) {
                    errorCode = 1;
                    return null;
                }

                userId = Convert.ToInt32(dUserInfo["id"]);
                if (PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId) != null) {
                    errorCode = 2;
                    PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId).Disconnect();
                    return null;
                }

                dbClient.SetQuery("SELECT `group`,`level`,`progress` FROM `user_achievements` WHERE `userid` = '" + userId + "'");
                dAchievements = dbClient.GetTable();

                dbClient.SetQuery("SELECT room_id FROM user_favorites WHERE `user_id` = '" + userId + "'");
                dFavouriteRooms = dbClient.GetTable();

                dbClient.SetQuery("SELECT `badge_id`,`badge_slot` FROM user_badges WHERE `user_id` = '" + userId + "'");
                dBadges = dbClient.GetTable();

                dbClient.SetQuery(
                    "SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online " +
                    "FROM users " +
                    "JOIN messenger_friendships " +
                    "ON users.id = messenger_friendships.user_one_id " +
                    "WHERE messenger_friendships.user_two_id = " + userId + " " +
                    "UNION ALL " +
                    "SELECT users.id,users.username,users.motto,users.look,users.last_online,users.hide_inroom,users.hide_online " +
                    "FROM users " +
                    "JOIN messenger_friendships " +
                    "ON users.id = messenger_friendships.user_two_id " +
                    "WHERE messenger_friendships.user_one_id = " + userId);
                dFriends = dbClient.GetTable();

                dbClient.SetQuery("SELECT messenger_requests.from_id,messenger_requests.to_id,users.username FROM users JOIN messenger_requests ON users.id = messenger_requests.from_id WHERE messenger_requests.to_id = " + userId);
                dRequests = dbClient.GetTable();

                dbClient.SetQuery("SELECT `quest_id`,`progress` FROM user_quests WHERE `user_id` = '" + userId + "'");
                dQuests = dbClient.GetTable();

                dbClient.SetQuery("SELECT `id`,`user_id`,`target`,`type` FROM `user_relationships` WHERE `user_id` = '" + userId + "'");
                dRelations = dbClient.GetTable();

                dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + userId + "' LIMIT 1");
                userInfo = dbClient.GetRow();
                if (userInfo == null) {
                    dbClient.RunQuery("INSERT INTO `user_info` (`user_id`) VALUES ('" + userId + "')");

                    dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + userId + "' LIMIT 1");
                    userInfo = dbClient.GetRow();
                }

                dbClient.RunQuery("UPDATE `users` SET `online` = '1' WHERE `id` = '" + userId + "' LIMIT 1");
            }

            ConcurrentDictionary<string, UserAchievement> achievements = new();
            foreach (DataRow dRow in dAchievements.Rows)
            {
                achievements.TryAdd(Convert.ToString(dRow["group"]), new UserAchievement(Convert.ToString(dRow["group"]), Convert.ToInt32(dRow["level"]), Convert.ToInt32(dRow["progress"])));
            }

            List<int> favouritedRooms = new();
            foreach (DataRow dRow in dFavouriteRooms.Rows)
            {
                favouritedRooms.Add(Convert.ToInt32(dRow["room_id"]));
            }

            List<Badge> badges = new();
            foreach (DataRow dRow in dBadges.Rows)
            {
                badges.Add(new Badge(Convert.ToString(dRow["badge_id"]), Convert.ToInt32(dRow["badge_slot"])));
            }

            Dictionary<int, MessengerBuddy> friends = new();
            foreach (DataRow dRow in dFriends.Rows)
            {
                int friendId = Convert.ToInt32(dRow["id"]);
                string friendName = Convert.ToString(dRow["username"]);
                string friendLook = Convert.ToString(dRow["look"]);
                string friendMotto = Convert.ToString(dRow["motto"]);
                int friendLastOnline = Convert.ToInt32(dRow["last_online"]);
                bool friendHideOnline = PlusEnvironment.EnumToBool(dRow["hide_online"].ToString());
                bool friendHideRoom = PlusEnvironment.EnumToBool(dRow["hide_inroom"].ToString());

                if (friendId == userId)
                    continue;

                if (!friends.ContainsKey(friendId))
                    friends.Add(friendId, new MessengerBuddy(friendId, friendName, friendLook, friendMotto, friendLastOnline, friendHideOnline, friendHideRoom));
            }

            Dictionary<int, MessengerRequest> requests = new();
            foreach (DataRow dRow in dRequests.Rows)
            {
                int receiverId = Convert.ToInt32(dRow["from_id"]);
                int senderId = Convert.ToInt32(dRow["to_id"]);

                string requestUsername = Convert.ToString(dRow["username"]);

                if (receiverId != userId) {
                    if (!requests.ContainsKey(receiverId))
                        requests.Add(receiverId, new MessengerRequest(userId, receiverId, requestUsername));
                } else {
                    if (!requests.ContainsKey(senderId))
                        requests.Add(senderId, new MessengerRequest(userId, senderId, requestUsername));
                }
            }

            Dictionary<int, int> quests = new();
            foreach (DataRow dRow in dQuests.Rows)
            {
                int questId = Convert.ToInt32(dRow["quest_id"]);

                if (quests.ContainsKey(questId))
                    quests.Remove(questId);

                quests.Add(questId, Convert.ToInt32(dRow["progress"]));
            }

            Dictionary<int, Relationship> relationships = new();
            foreach (DataRow row in dRelations.Rows)
            {
                if (friends.ContainsKey(Convert.ToInt32(row[2])))
                    relationships.Add(Convert.ToInt32(row[2]), new Relationship(Convert.ToInt32(row[0]), Convert.ToInt32(row[2]), Convert.ToInt32(row[3].ToString())));
            }

            Habbo user = HabboFactory.GenerateHabbo(dUserInfo, userInfo);

            dAchievements = null;
            dFavouriteRooms = null;
            dBadges = null;
            dFriends = null;
            dRequests = null;
            dRelations = null;

            errorCode = 0;
            return new UserData(userId, achievements, favouritedRooms, badges, friends, requests, quests, user, relationships);
        }

        public static UserData GetUserData(int userId)
        {
            GameClients.GameClient onlineClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
            if (onlineClient?.GetHabbo() != null)
                return new UserData(userId, new ConcurrentDictionary<string, UserAchievement>(), new List<int>(), new List<Badge>(), new Dictionary<int, MessengerBuddy>(), new Dictionary<int, MessengerRequest>(), new Dictionary<int, int>(), onlineClient.GetHabbo(), onlineClient.GetHabbo().Relationships ?? new Dictionary<int, Relationship>());

            DataRow dUserInfo = null;
            DataRow userInfo = null;
            DataTable dRelations = null;
            DataTable dGroups = null;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`username`,`rank`,`motto`,`look`,`gender`,`last_online`,`credits`,`activity_points`,`home_room`,`home_room_data`,`block_newfriends`,`hide_online`,`hide_inroom`,`vip`,`account_created`,`vip_points`,`machine_id`,`volume`,`chat_preference`, `focus_preference`, `pets_muted`,`bots_muted`,`advertising_report_blocked`,`last_change`,`gotw_points`,`ignore_invites`,`time_muted`,`allow_gifts`,`friend_bar_state`,`disable_forced_effects`,`allow_mimic`,`rank_vip`,`bubble` FROM `users` WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("id", userId);
                dUserInfo = dbClient.GetRow();

                if (dUserInfo == null)
                    return null;


                dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + userId + "' LIMIT 1");
                userInfo = dbClient.GetRow();
                if (userInfo == null) {
                    dbClient.RunQuery("INSERT INTO `user_info` (`user_id`) VALUES ('" + userId + "')");

                    dbClient.SetQuery("SELECT * FROM `user_info` WHERE `user_id` = '" + userId + "' LIMIT 1");
                    userInfo = dbClient.GetRow();
                }

                dbClient.SetQuery("SELECT group_id,level FROM group_memberships WHERE user_id=@id");
                dbClient.AddParameter("id", userId);
                dGroups = dbClient.GetTable();

                dbClient.SetQuery("SELECT `id`,`target`,`type` FROM user_relationships WHERE user_id=@id");
                dbClient.AddParameter("id", userId);
                dRelations = dbClient.GetTable();
            }

            ConcurrentDictionary<string, UserAchievement> achievements = new();
            List<int> favouritedRooms = new();
            List<Badge> badges = new();
            Dictionary<int, MessengerBuddy> friends = new();
            Dictionary<int, MessengerRequest> friendRequests = new();
            Dictionary<int, int> quests = new();

            Dictionary<int, Relationship> relationships = new();
            foreach (DataRow row in dRelations.Rows)
            {
                if (!relationships.ContainsKey(Convert.ToInt32(row["id"]))) {
                    relationships.Add(Convert.ToInt32(row["target"]), new Relationship(Convert.ToInt32(row["id"]), Convert.ToInt32(row["target"]), Convert.ToInt32(row["type"].ToString())));
                }
            }

            Habbo user = HabboFactory.GenerateHabbo(dUserInfo, userInfo);
            return new UserData(userId, achievements, favouritedRooms, badges, friends, friendRequests, quests, user, relationships);
        }
    }
}
