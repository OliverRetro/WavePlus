using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.Users.Authenticator;
using Plus.HabboHotel.Users.Badges;
using Plus.HabboHotel.Users.Messenger;
using Plus.HabboHotel.Users.Relationships;
using MessengerRequest = Plus.HabboHotel.Users.Messenger.MessengerRequest;
using UserAchievement = Plus.HabboHotel.Achievements.UserAchievement;

namespace Plus.HabboHotel.Users.UserData
{
    public static class UserDataFactory
    {
        public static UserData GetUserData(string sessionTicket, out byte errorCode)
        {
            using WavePlusContext db = PlusEnvironment.GetDbContext();

            UserEntity dUserInfo = db.Users.FirstOrDefault(u => u.AuthTicket == sessionTicket);
            if (dUserInfo == null) {
                errorCode = 1;
                return null;
            }

            int userId = dUserInfo.Id;
            uint uid = (uint)userId;

            GameClients.GameClient existing = PlusEnvironment.GetGame().GetClientManager().GetClientByUserId(userId);
            if (existing != null) {
                // A login is colliding with an existing session for this user. This is almost always
                // a reconnect (page reload / socket re-open), so we take over rather than reject —
                // otherwise a reload would kick the user out entirely.
                //
                //  - If the old session is already a ghost (its socket closed and it's sitting in the
                //    disconnect grace window), finalize it now: persist its cached state and free the
                //    room presence.
                //  - Otherwise the old socket hasn't been noticed as closed yet (the reload race:
                //    the new connection authenticated before the old one's ChannelInactive fired).
                //    Tear it down immediately (no new ghost) so this fresh login can take its place.
                //
                // Either way we FALL THROUGH to load fresh DB state for the new connection instead of
                // aborting with errorCode 2.
                if (PlusEnvironment.GetDisconnectDelayManager()?.IsGhost(userId) == true)
                    PlusEnvironment.GetDisconnectDelayManager().FinalizeNow(userId);
                else
                    existing.Disconnect(immediate: true);
                db.Entry(dUserInfo).Reload();
            }

            ConcurrentDictionary<string, UserAchievement> achievements = new();
            foreach (var a in db.UserAchievements.Where(a => a.Userid == uid)
                         .Select(a => new { a.Group, a.Level, a.Progress }).ToList()) {
                achievements.TryAdd(a.Group, new UserAchievement(a.Group, a.Level, a.Progress));
            }

            List<int> favouritedRooms = db.UserFavorites.Where(f => f.UserId == uid)
                .Select(f => (int)f.RoomId).ToList();

            List<Badge> badges = db.UserBadges.Where(b => b.UserId == uid)
                .Select(b => new { b.BadgeId, b.BadgeSlot }).ToList()
                .Select(b => new Badge(b.BadgeId, b.BadgeSlot)).ToList();

            // Friends: users linked from either side of a friendship row.
            List<int> friendIds = db.MessengerFriendships.Where(f => f.UserTwoId == uid).Select(f => (int)f.UserOneId)
                .Concat(db.MessengerFriendships.Where(f => f.UserOneId == uid).Select(f => (int)f.UserTwoId))
                .Distinct().ToList();

            Dictionary<int, MessengerBuddy> friends = new();
            foreach (var fr in db.Users.Where(u => friendIds.Contains(u.Id))
                         .Select(u => new { u.Id, u.Username, u.Look, u.Motto, u.LastOnline, u.HideOnline, u.HideInroom }).ToList()) {
                if (fr.Id == userId)
                    continue;

                if (!friends.ContainsKey(fr.Id))
                    friends.Add(fr.Id, new MessengerBuddy(fr.Id, fr.Username, fr.Look, fr.Motto, fr.LastOnline ?? 0,
                        PlusEnvironment.EnumToBool(fr.HideOnline), PlusEnvironment.EnumToBool(fr.HideInroom)));
            }

            Dictionary<int, MessengerRequest> requests = new();
            foreach (var rr in (from r in db.MessengerRequests
                                where r.ToId == uid
                                join u in db.Users on (int)r.FromId equals u.Id
                                select new { from_id = (int)r.FromId, to_id = (int)r.ToId, u.Username }).ToList()) {
                int receiverId = rr.from_id;
                int senderId = rr.to_id;

                if (receiverId != userId) {
                    if (!requests.ContainsKey(receiverId))
                        requests.Add(receiverId, new MessengerRequest(userId, receiverId, rr.Username));
                } else {
                    if (!requests.ContainsKey(senderId))
                        requests.Add(senderId, new MessengerRequest(userId, senderId, rr.Username));
                }
            }

            Dictionary<int, int> quests = new();
            foreach (var q in db.UserQuests.Where(q => q.UserId == uid)
                         .Select(q => new { q.QuestId, q.Progress }).ToList()) {
                quests[(int)q.QuestId] = q.Progress ?? 0;
            }

            UserInfoEntity userInfo = db.UserInfos.FirstOrDefault(i => i.UserId == userId);
            if (userInfo == null) {
                userInfo = new UserInfoEntity { UserId = userId };
                db.UserInfos.Add(userInfo);
                db.SaveChanges();
            }

            Dictionary<int, Relationship> relationships = new();
            foreach (var rel in db.UserRelationships.Where(r => r.UserId == userId)
                         .Select(r => new { r.Id, r.Target, r.Type }).ToList()) {
                if (friends.ContainsKey(rel.Target))
                    relationships.Add(rel.Target, new Relationship(rel.Id, rel.Target, Convert.ToInt32(rel.Type)));
            }

            dUserInfo.Online = "1";
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

            UserEntity dUserInfo = db.Users.FirstOrDefault(u => u.Id == userId);
            if (dUserInfo == null)
                return null;

            UserInfoEntity userInfo = db.UserInfos.FirstOrDefault(i => i.UserId == userId);
            if (userInfo == null) {
                userInfo = new UserInfoEntity { UserId = userId };
                db.UserInfos.Add(userInfo);
                db.SaveChanges();
            }

            ConcurrentDictionary<string, UserAchievement> achievements = new();
            List<int> favouritedRooms = new();
            List<Badge> badges = new();
            Dictionary<int, MessengerBuddy> friends = new();
            Dictionary<int, MessengerRequest> friendRequests = new();
            Dictionary<int, int> quests = new();

            Dictionary<int, Relationship> relationships = new();
            foreach (var rel in db.UserRelationships.Where(r => r.UserId == userId)
                         .Select(r => new { r.Id, r.Target, r.Type }).ToList()) {
                if (!relationships.ContainsKey(rel.Id))
                    relationships.Add(rel.Target, new Relationship(rel.Id, rel.Target, Convert.ToInt32(rel.Type)));
            }

            Habbo habbo = HabboFactory.GenerateHabbo(dUserInfo, userInfo);
            return new UserData(userId, achievements, favouritedRooms, badges, friends, friendRequests, quests, habbo, relationships);
        }
    }
}