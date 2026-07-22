using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Plus.Communication.Packets.Outgoing;
using Plus.Communication.Packets.Outgoing.Messenger;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Microsoft.EntityFrameworkCore;
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

            _requests = [];
            _friends = [];
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
            uint uid = (uint)_userId;
            using WavePlusContext db = PlusEnvironment.GetDbContext();

            var messages = db.MessengerOfflineMessages.Where(m => m.ToId == uid)
                .Select(m => new { m.FromId, m.Message, m.Timestamp }).ToList();

            GameClient client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(_userId);
            if (client == null)
                return;

            foreach (var row in messages)
                client.SendPacket(new NewConsoleMessageComposer((int)row.FromId, row.Message, (int)(UnixTimestamp.GetNow() - (int)row.Timestamp)));

            // db.messenger_offline_messages.Where(m => m.to_id == uid).ExecuteDelete();
        }

        public void Destroy()
        {
            IEnumerable<GameClient> onlineUsers = PlusEnvironment.GetGame().GetClientManager().GetClientsById(_friends.Keys);

            foreach (GameClient client in onlineUsers) {
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

            foreach (GameClient client in onlineUsers.ToList()) {
                try {
                    if (client == null || client.GetHabbo() == null || client.GetHabbo().GetMessenger() == null)
                        continue;

                    client.GetHabbo().GetMessenger().UpdateFriend(_userId, client, true);

                    if (client.GetHabbo() == null)
                        continue;

                    UpdateFriend(client.GetHabbo().Id, client, notification);
                } catch (Exception e) {
                    Console.WriteLine(e.ToString());
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
            uint uid = (uint)_userId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.MessengerRequests.Where(r => r.FromId == uid || r.ToId == uid).ExecuteDelete();

            ClearRequests();
        }

        public void HandleRequest(int sender)
        {
            uint uid = (uint)_userId;
            uint snd = (uint)sender;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.MessengerRequests.Where(r => (r.FromId == uid && r.ToId == snd) || (r.ToId == uid && r.FromId == snd)).ExecuteDelete();

            _requests.Remove(sender);
        }

        public void CreateFriendship(int friendId)
        {
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.MessengerFriendships.Upsert(new MessengerFriendshipEntity
                {
                    UserOneId = (uint)_userId,
                    UserTwoId = (uint)friendId
                }).On(f => new { f.UserOneId, f.UserTwoId }).Run();

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
            uint uid = (uint)_userId;
            uint fid = (uint)friendId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.MessengerFriendships.Where(f => (f.UserOneId == uid && f.UserTwoId == fid) || (f.UserTwoId == uid && f.UserOneId == fid)).ExecuteDelete();

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
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    var dRow = db.Users.Where(u => u.Id == friendId)
                        .Select(u => new { u.Username, u.Motto, u.Look, u.LastOnline, u.HideInroom, u.HideOnline })
                        .FirstOrDefault();

                    newFriend = new MessengerBuddy(friendId, dRow.Username, dRow.Look, dRow.Motto, dRow.LastOnline ?? 0,
                        PlusEnvironment.EnumToBool(dRow.HideOnline), PlusEnvironment.EnumToBool(dRow.HideInroom));
                }
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

            uint uid = (uint)_userId;
            uint rid = (uint)requestId;
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                return db.MessengerFriendships.Any(f => f.UserOneId == uid && f.UserTwoId == rid);
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
                string query = userQuery.ToLower();
                int foundId;
                string foundBlock;
                bool found;
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    var row = db.Users.Where(u => u.Username == query).Select(u => new { u.Id, u.BlockNewfriends }).FirstOrDefault();
                    found = row != null;
                    foundId = row?.Id ?? 0;
                    foundBlock = row?.BlockNewfriends;
                }

                if (!found)
                    return false;

                userId = foundId;
                hasFqDisabled = PlusEnvironment.EnumToBool(foundBlock);
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

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
                db.MessengerRequests.Upsert(new Plus.Database.EF.Entities.MessengerRequestEntity
                {
                    FromId = (uint)_userId,
                    ToId = (uint)toId
                }).On(r => new { r.FromId, r.ToId }).Run();

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
                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                    db.MessengerOfflineMessages.Add(new Database.EF.Entities.MessengerOfflineMessageEntity
                    {
                        ToId = (uint)toId,
                        FromId = (uint)GetClient().GetHabbo().Id,
                        Message = message,
                        Timestamp = PlusEnvironment.GetUnixTimestamp()
                    });
                    db.SaveChanges();
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
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.ChatlogsConsoles.Add(new Database.EF.Entities.ChatlogsConsoleEntity
                {
                    FromId = (uint)fromId,
                    ToId = (uint)toId,
                    Message = message,
                    Timestamp = PlusEnvironment.GetUnixTimestamp()
                });
                db.SaveChanges();
            }
        }

        public MessageComposer SerializeUpdate(MessengerBuddy friend)
        {
            return new FriendListUpdateComposer(GetClient().GetHabbo(), friend);
        }

        public void BroadcastAchievement(int userId, MessengerEventTypes type, string data)
        {
            IEnumerable<GameClient> myFriends = PlusEnvironment.GetGame().GetClientManager().GetClientsById(_friends.Keys);

            foreach (GameClient client in myFriends.ToList()) {
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