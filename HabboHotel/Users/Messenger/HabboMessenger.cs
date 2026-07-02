using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Cache.Type;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;
using Plus.Utilities;

namespace Plus.HabboHotel.Users.Messenger
{
    public class HabboMessenger
    {
        public bool AppearOffline;
        private readonly int _userId;

        private Dictionary<int, MessengerBuddy> _friends;
        private Dictionary<int, MessengerRequest> _requests;

        public HabboMessenger(int userId)
        {
            _userId = userId;

            _requests = new Dictionary<int, MessengerRequest>();
            _friends = new Dictionary<int, MessengerBuddy>();
        }

        public void Init(Dictionary<int, MessengerBuddy> friends, Dictionary<int, MessengerRequest> requests)
        {
            _requests = new Dictionary<int, MessengerRequest>(requests);
            _friends = new Dictionary<int, MessengerBuddy>(friends);
        }

        public bool TryGetRequest(int senderId, out MessengerRequest request)
        {
            return _requests.TryGetValue(senderId, out request);
        }

        public bool TryGetFriend(int userId, out MessengerBuddy buddy)
        {
            return _friends.TryGetValue(userId, out buddy);
        }

        public void ProcessOfflineMessages()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `messenger_offline_messages` WHERE `to_id` = @id;");
                dbClient.AddParameter("id", _userId);
                DataTable getMessages = dbClient.GetTable();

                if (getMessages != null) {
                    GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(_userId);
                    if (client == null)
                        return;

                    foreach (DataRow row in getMessages.Rows)
                    {
                        client.SendPacket(new NewConsoleMessageComposer(Convert.ToInt32(row["from_id"]), Convert.ToString(row["message"]), (int) (UnixTimestamp.GetNow() - Convert.ToInt32(row["timestamp"]))));
                    }

                    dbClient.SetQuery("DELETE FROM `messenger_offline_messages` WHERE `to_id` = @id");
                    dbClient.AddParameter("id", _userId);
                    dbClient.RunQuery();
                }
            }
        }

        public void Destroy()
        {
            IEnumerable<GameClient> onlineUsers = PlusEnvironment.GetGame().GetClientManager().GetClientsById(_friends.Keys);

            foreach (GameClient client in onlineUsers)
            {
                if (client.GetHabbo() == null || client.GetHabbo().GetMessenger() == null)
                    continue;

                client.GetHabbo().GetMessenger().UpdateFriend(_userId, null, true);
            }
        }

        public void OnStatusChanged(bool notification)
        {
            if (GetClient() == null || GetClient().GetHabbo() == null || GetClient().GetHabbo().GetMessenger() == null)
                return;

            if (_friends == null)
                return;

            IEnumerable<GameClient> onlineUsers = PlusEnvironment.GetGame().GetClientManager().GetClientsById(_friends.Keys);
            if (onlineUsers.Count() == 0)
                return;

            foreach (GameClient client in onlineUsers.ToList())
            {
                try
                {
                    if (client == null || client.GetHabbo() == null || client.GetHabbo().GetMessenger() == null)
                        continue;

                    client.GetHabbo().GetMessenger().UpdateFriend(_userId, client, true);

                    if (client.GetHabbo() == null)
                        continue;

                    UpdateFriend(client.GetHabbo().Id, client, notification);
                }
                catch
                {
                }
            }
        }

        public void UpdateFriend(int userId, GameClient client, bool notification)
        {
            if (_friends.ContainsKey(userId)) {
                _friends[userId].UpdateUser(client);

                if (notification) {
                    GameClient userClient = GetClient();
                    userClient?.SendPacket(SerializeUpdate(_friends[userId]));
                }
            }
        }

        public void HandleAllRequests()
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM messenger_requests WHERE from_id = " + _userId + " OR to_id = " + _userId);
            }

            ClearRequests();
        }

        public void HandleRequest(int sender)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM messenger_requests WHERE (from_id = " + _userId + " AND to_id = " + sender + ") OR (to_id = " + _userId + " AND from_id = " + sender + ")");
            }

            _requests.Remove(sender);
        }

        public void CreateFriendship(int friendId)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("REPLACE INTO messenger_friendships (user_one_id,user_two_id) VALUES (" + _userId + "," + friendId + ")");
            }

            OnNewFriendship(friendId);

            GameClient user = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(friendId);

            if (user != null && user.GetHabbo().GetMessenger() != null) {
                user.GetHabbo().GetMessenger().OnNewFriendship(_userId);
            }

            if (user != null)
                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(user, "ACH_FriendListSize", 1);

            GameClient thisUser = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(_userId);
            if (thisUser != null)
                PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(thisUser, "ACH_FriendListSize", 1);
        }

        public void DestroyFriendship(int friendId)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM messenger_friendships WHERE (user_one_id = " + _userId + " AND user_two_id = " + friendId + ") OR (user_two_id = " + _userId + " AND user_one_id = " + friendId + ")");
            }

            OnDestroyFriendship(friendId);

            GameClient user = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(friendId);

            if (user != null && user.GetHabbo().GetMessenger() != null)
                user.GetHabbo().GetMessenger().OnDestroyFriendship(_userId);
        }

        public void OnNewFriendship(int friendId)
        {
            GameClient friend = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(friendId);

            MessengerBuddy newFriend;
            if (friend == null || friend.GetHabbo() == null) {
                DataRow dRow;
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT id,username,motto,look,last_online,hide_inroom,hide_online FROM users WHERE `id` = @friendid LIMIT 1");
                    dbClient.AddParameter("friendid", friendId);
                    dRow = dbClient.GetRow();
                }

                newFriend = new MessengerBuddy(friendId, Convert.ToString(dRow["username"]), Convert.ToString(dRow["look"]), Convert.ToString(dRow["motto"]), Convert.ToInt32(dRow["last_online"]),
                    PlusEnvironment.EnumToBool(dRow["hide_online"].ToString()), PlusEnvironment.EnumToBool(dRow["hide_inroom"].ToString()));
            } else {
                Habbo user = friend.GetHabbo();

                newFriend = new MessengerBuddy(friendId, user.Username, user.Look, user.Motto, 0, user.AppearOffline, user.AllowPublicRoomStatus);
                newFriend.UpdateUser(friend);
            }

            if (!_friends.ContainsKey(friendId))
                _friends.Add(friendId, newFriend);

            GetClient().SendPacket(SerializeUpdate(newFriend));
        }

        public bool RequestExists(int requestId)
        {
            if (_requests.ContainsKey(requestId))
                return true;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT user_one_id FROM messenger_friendships WHERE user_one_id = @myID AND user_two_id = @friendID");
                dbClient.AddParameter("myID", Convert.ToInt32(_userId));
                dbClient.AddParameter("friendID", Convert.ToInt32(requestId));
                return dbClient.FindsResult();
            }
        }

        public bool FriendshipExists(int friendId)
        {
            return _friends.ContainsKey(friendId);
        }

        public void OnDestroyFriendship(int friend)
        {
            if (_friends.ContainsKey(friend))
                _friends.Remove(friend);

            GetClient().SendPacket(new FriendListUpdateComposer(friend));
        }

        public bool RequestBuddy(string userQuery)
        {
            int userId;
            bool hasFqDisabled;

            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(userQuery);
            if (client == null) {
                DataRow row;
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `id`,`block_newfriends` FROM `users` WHERE `username` = @query LIMIT 1");
                    dbClient.AddParameter("query", userQuery.ToLower());
                    row = dbClient.GetRow();
                }

                if (row == null)
                    return false;

                userId = Convert.ToInt32(row["id"]);
                hasFqDisabled = PlusEnvironment.EnumToBool(row["block_newfriends"].ToString());
            } else {
                userId = client.GetHabbo().Id;
                hasFqDisabled = client.GetHabbo().AllowFriendRequests;
            }

            if (hasFqDisabled) {
                GetClient().SendPacket(new MessengerErrorComposer(39, 3));
                return false;
            }

            int toId = userId;
            if (RequestExists(toId))
                return true;

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("REPLACE INTO `messenger_requests` (`from_id`,`to_id`) VALUES ('" + _userId + "','" + toId + "')");
            }

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(GetClient(), QuestType.AddFriends);

            GameClient toUser = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(toId);
            if (toUser == null || toUser.GetHabbo() == null)
                return true;

            MessengerRequest request = new(toId, _userId, PlusEnvironment.GetGame().GetClientManager().GetNameById(_userId));

            toUser.GetHabbo().GetMessenger().OnNewRequest(_userId);

            UserCache thisUser = PlusEnvironment.GetGame().GetCacheManager().GenerateUser(_userId);

            if (thisUser != null)
                toUser.SendPacket(new NewBuddyRequestComposer(thisUser));

            _requests.Add(toId, request);
            return true;
        }

        public void OnNewRequest(int friendId)
        {
            if (!_requests.ContainsKey(friendId))
                _requests.Add(friendId, new MessengerRequest(_userId, friendId, PlusEnvironment.GetGame().GetClientManager().GetNameById(friendId)));
        }

        public void SendInstantMessage(int toId, string message)
        {
            if (toId == 0)
                return;

            if (GetClient() == null)
                return;

            if (GetClient().GetHabbo() == null)
                return;

            if (!FriendshipExists(toId)) {
                GetClient().SendPacket(new InstantMessageErrorComposer(MessengerMessageErrors.NotFriends, toId));
                return;
            }

            if (GetClient().GetHabbo().MessengerSpamCount >= 12) {
                GetClient().GetHabbo().MessengerSpamTime = PlusEnvironment.GetUnixTimestamp() + 60;
                GetClient().GetHabbo().MessengerSpamCount = 0;
                GetClient().SendNotification("You cannot send a message, you have flooded the console.\n\nYou can send a message in 60 seconds.");
                return;
            }

            if (GetClient().GetHabbo().MessengerSpamTime > PlusEnvironment.GetUnixTimestamp()) {
                double time = GetClient().GetHabbo().MessengerSpamTime - PlusEnvironment.GetUnixTimestamp();
                GetClient().SendNotification("You cannot send a message, you have flooded the console.\n\nYou can send a message in " + time + " seconds.");
                return;
            }
            
            GetClient().GetHabbo().MessengerSpamCount++;

            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(toId);
            if (client == null || client.GetHabbo() == null || client.GetHabbo().GetMessenger() == null) {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("INSERT INTO `messenger_offline_messages` (`to_id`, `from_id`, `message`, `timestamp`) VALUES (@tid, @fid, @msg, UNIX_TIMESTAMP())");
                    dbClient.AddParameter("tid", toId);
                    dbClient.AddParameter("fid", GetClient().GetHabbo().Id);
                    dbClient.AddParameter("msg", message);
                    dbClient.RunQuery();
                }

                return;
            }

            if (!client.GetHabbo().AllowConsoleMessages || client.GetHabbo().GetIgnores().IgnoredUserIds().Contains(GetClient().GetHabbo().Id)) {
                GetClient().SendPacket(new InstantMessageErrorComposer(MessengerMessageErrors.FriendBusy, toId));
                return;
            }

            if (GetClient().GetHabbo().TimeMuted > 0) {
                GetClient().SendPacket(new InstantMessageErrorComposer(MessengerMessageErrors.YourMuted, toId));
                return;
            }

            if (client.GetHabbo().TimeMuted > 0) {
                GetClient().SendPacket(new InstantMessageErrorComposer(MessengerMessageErrors.FriendMuted, toId));
            }

            if (string.IsNullOrEmpty(message))
                return;

            client.SendPacket(new NewConsoleMessageComposer(_userId, message));
            LogPm(_userId, toId, message);
        }

        public void LogPm(int fromId, int toId, string message)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO chatlogs_console VALUES (NULL, @fromId, @toId, @message, UNIX_TIMESTAMP())");
                dbClient.AddParameter("fromId", fromId);
                dbClient.AddParameter("toId", toId);
                dbClient.AddParameter("message", message);
                dbClient.RunQuery();
            }
        }

        public MessageComposer SerializeUpdate(MessengerBuddy friend)
        {
            return new FriendListUpdateComposer(GetClient().GetHabbo(), friend);
        }

        public void BroadcastAchievement(int userId, MessengerEventTypes type, string data)
        {
            IEnumerable<GameClient> myFriends = PlusEnvironment.GetGame().GetClientManager().GetClientsById(_friends.Keys);

            foreach (GameClient client in myFriends.ToList())
            {
                if (client.GetHabbo() != null && client.GetHabbo().GetMessenger() != null) {
                    client.SendPacket(new FriendNotificationComposer(userId, type, data));
                    client.GetHabbo().GetMessenger().OnStatusChanged(true);
                }
            }
        }

        public void ClearRequests()
        {
            _requests.Clear();
        }

        private GameClient GetClient()
        {
            return PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(_userId);
        }

        public ICollection<MessengerRequest> GetRequests()
        {
            return _requests.Values;
        }

        public ICollection<MessengerBuddy> GetFriends()
        {
            return _friends.Values;
        }
    }
}