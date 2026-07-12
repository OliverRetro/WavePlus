using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
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
            using WavePlusContext db = PlusEnvironment.GetDbContext();

            user dUserInfo = db.users.FirstOrDefault(u => u.auth_ticket == sessionTicket);
            if (dUserInfo == null) {
                errorCode = 1;
                return null;
            }

            int userId = dUserInfo.id;
            uint uid = (uint)userId;

            GameClients.GameClient existing = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
            if (existing != null) {
                errorCode = 2;
                existing.Disconnect();
                return null;
            }

            ConcurrentDictionary<string, UserAchievement> achievements = new();
            foreach (var a in db.user_achievements.Where(a => a.userid == uid)
                         .Select(a => new { a.group, a.level, a.progress }).ToList())
            {
                achievements.TryAdd(a.group, new UserAchievement(a.group, a.level, a.progress));
            }

            List<int> favouritedRooms = db.user_favorites.Where(f => f.user_id == uid)
                .Select(f => (int)f.room_id).ToList();

            List<Badge> badges = db.user_badges.Where(b => b.user_id == uid)
                .Select(b => new { b.badge_id, b.badge_slot }).ToList()
                .Select(b => new Badge(b.badge_id, b.badge_slot)).ToList();

            // Friends: users linked from either side of a friendship row.
            List<int> friendIds = db.messenger_friendships.Where(f => f.user_two_id == uid).Select(f => (int)f.user_one_id)
                .Concat(db.messenger_friendships.Where(f => f.user_one_id == uid).Select(f => (int)f.user_two_id))
                .Distinct().ToList();

            Dictionary<int, MessengerBuddy> friends = new();
            foreach (var fr in db.users.Where(u => friendIds.Contains(u.id))
                         .Select(u => new { u.id, u.username, u.look, u.motto, u.last_online, u.hide_online, u.hide_inroom }).ToList())
            {
                if (fr.id == userId)
                    continue;

                if (!friends.ContainsKey(fr.id))
                    friends.Add(fr.id, new MessengerBuddy(fr.id, fr.username, fr.look, fr.motto, fr.last_online ?? 0,
                        PlusEnvironment.EnumToBool(fr.hide_online), PlusEnvironment.EnumToBool(fr.hide_inroom)));
            }

            Dictionary<int, MessengerRequest> requests = new();
            foreach (var rr in (from r in db.messenger_requests
                                where r.to_id == uid
                                join u in db.users on (int)r.from_id equals u.id
                                select new { from_id = (int)r.from_id, to_id = (int)r.to_id, u.username }).ToList())
            {
                int receiverId = rr.from_id;
                int senderId = rr.to_id;

                if (receiverId != userId) {
                    if (!requests.ContainsKey(receiverId))
                        requests.Add(receiverId, new MessengerRequest(userId, receiverId, rr.username));
                } else {
                    if (!requests.ContainsKey(senderId))
                        requests.Add(senderId, new MessengerRequest(userId, senderId, rr.username));
                }
            }

            Dictionary<int, int> quests = new();
            foreach (var q in db.user_quests.Where(q => q.user_id == uid)
                         .Select(q => new { q.quest_id, q.progress }).ToList())
            {
                quests[(int)q.quest_id] = q.progress ?? 0;
            }

            user_info userInfo = db.user_infos.FirstOrDefault(i => i.user_id == userId);
            if (userInfo == null) {
                userInfo = new user_info { user_id = userId };
                db.user_infos.Add(userInfo);
                db.SaveChanges();
            }

            Dictionary<int, Relationship> relationships = new();
            foreach (var rel in db.user_relationships.Where(r => r.user_id == userId)
                         .Select(r => new { r.id, r.target, r.type }).ToList())
            {
                if (friends.ContainsKey(rel.target))
                    relationships.Add(rel.target, new Relationship(rel.id, rel.target, Convert.ToInt32(rel.type)));
            }

            dUserInfo.online = "1";
            db.SaveChanges();

            Habbo habbo = HabboFactory.GenerateHabbo(dUserInfo, userInfo);

            errorCode = 0;
            return new UserData(userId, achievements, favouritedRooms, badges, friends, requests, quests, habbo, relationships);
        }

        public static UserData GetUserData(int userId)
        {
            GameClients.GameClient onlineClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
            if (onlineClient?.GetHabbo() != null)
                return new UserData(userId, new ConcurrentDictionary<string, UserAchievement>(), new List<int>(), new List<Badge>(), new Dictionary<int, MessengerBuddy>(), new Dictionary<int, MessengerRequest>(), new Dictionary<int, int>(), onlineClient.GetHabbo(), onlineClient.GetHabbo().Relationships ?? new Dictionary<int, Relationship>());

            using WavePlusContext db = PlusEnvironment.GetDbContext();

            user dUserInfo = db.users.FirstOrDefault(u => u.id == userId);
            if (dUserInfo == null)
                return null;

            user_info userInfo = db.user_infos.FirstOrDefault(i => i.user_id == userId);
            if (userInfo == null) {
                userInfo = new user_info { user_id = userId };
                db.user_infos.Add(userInfo);
                db.SaveChanges();
            }

            ConcurrentDictionary<string, UserAchievement> achievements = new();
            List<int> favouritedRooms = new();
            List<Badge> badges = new();
            Dictionary<int, MessengerBuddy> friends = new();
            Dictionary<int, MessengerRequest> friendRequests = new();
            Dictionary<int, int> quests = new();

            Dictionary<int, Relationship> relationships = new();
            foreach (var rel in db.user_relationships.Where(r => r.user_id == userId)
                         .Select(r => new { r.id, r.target, r.type }).ToList())
            {
                if (!relationships.ContainsKey(rel.id))
                    relationships.Add(rel.target, new Relationship(rel.id, rel.target, Convert.ToInt32(rel.type)));
            }

            Habbo habbo = HabboFactory.GenerateHabbo(dUserInfo, userInfo);
            return new UserData(userId, achievements, favouritedRooms, badges, friends, friendRequests, quests, habbo, relationships);
        }
    }
}
