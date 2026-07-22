using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing;
using Plus.Database.EF;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Messenger;

namespace Plus.HabboHotel.Navigator
{
    internal static class NavigatorHandler
    {
        public static void Search(ServerPacket packet, SearchResultList result, string query, Habbo habbo, int limit)
        {
            if (habbo == null)
                return;

            switch (result.CategoryType) {
                default:
                case NavigatorCategoryType.MyFavourites:
                case NavigatorCategoryType.MyHistory:
                case NavigatorCategoryType.Featured:
                    packet.WriteInteger(0);
                    break;

                case NavigatorCategoryType.Query: {
                        #region Query

                        if (query.ToLower().StartsWith("owner:")) {
                            if (query.Length > 0) {
                                List<int> roomIds;
                                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                                    string username = query.Remove(0, 6);
                                    int userId = db.Users.Where(u => u.Username == username).Select(u => u.Id).FirstOrDefault();
                                    string ownerId = userId.ToString();
                                    roomIds = db.Rooms.Where(r => r.Owner == ownerId && r.State != "invisible").OrderByDescending(r => r.UsersNow).Take(50).Select(r => r.Id).ToList();
                                }

                                List<RoomData> results = new();
                                foreach (int roomId in roomIds) {
                                    if (!RoomFactory.TryGetData(roomId, out RoomData data))
                                        continue;

                                    if (!results.Contains(data))
                                        results.Add(data);
                                }

                                packet.WriteInteger(results.Count);
                                foreach (RoomData data in results.ToList()) {
                                    RoomAppender.WriteRoom(packet, data, data.Promotion);
                                }
                            }
                        }
                        else if (query.ToLower().StartsWith("tag:")) {
                            query = query.Remove(0, 4);
                            ICollection<Room> tagMatches = PlusEnvironment.GetGame().GetRoomManager().SearchTaggedRooms(query);

                            packet.WriteInteger(tagMatches.Count);
                            foreach (RoomData data in tagMatches.ToList()) {
                                RoomAppender.WriteRoom(packet, data, data.Promotion);
                            }
                        }
                        else if (query.ToLower().StartsWith("group:")) {
                            query = query.Remove(0, 6);
                            ICollection<Room> groupRooms = PlusEnvironment.GetGame().GetRoomManager().SearchGroupRooms(query);

                            packet.WriteInteger(groupRooms.Count);
                            foreach (RoomData data in groupRooms.ToList()) {
                                RoomAppender.WriteRoom(packet, data, data.Promotion);
                            }
                        }
                        else {
                            if (query.Length > 0) {
                                string like = query + "%";
                                var rows = new List<(int Id, string State)>();
                                using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                                    rows = db.Rooms.Where(r => EF.Functions.Like(r.Caption, like)).OrderByDescending(r => r.UsersNow).Take(50).Select(r => new { r.Id, r.State }).AsEnumerable().Select(r => (r.Id, r.State)).ToList();
                                }

                                List<RoomData> results = new();
                                foreach ((int Id, string State) row in rows) {
                                    if (row.State == "invisible")
                                        continue;

                                    if (!RoomFactory.TryGetData(row.Id, out RoomData data))
                                        continue;

                                    if (!results.Contains(data))
                                        results.Add(data);
                                }

                                packet.WriteInteger(results.Count);
                                foreach (RoomData data in results.ToList()) {
                                    RoomAppender.WriteRoom(packet, data, data.Promotion);
                                }
                            }
                        }

                        #endregion

                        break;
                    }

                case NavigatorCategoryType.Popular: {
                        List<Room> popularRooms = PlusEnvironment.GetGame().GetRoomManager().GetPopularRooms(-1, limit);

                        packet.WriteInteger(popularRooms.Count);
                        foreach (RoomData data in popularRooms.ToList()) {
                            RoomAppender.WriteRoom(packet, data, data.Promotion);
                        }

                        break;
                    }

                case NavigatorCategoryType.Recommended: {
                        List<Room> recommendedRooms = PlusEnvironment.GetGame().GetRoomManager().GetRecommendedRooms(limit);

                        packet.WriteInteger(recommendedRooms.Count);
                        foreach (RoomData data in recommendedRooms.ToList()) {
                            RoomAppender.WriteRoom(packet, data, data.Promotion);
                        }

                        break;
                    }

                case NavigatorCategoryType.Category: {
                        List<Room> getRoomsByCategory = PlusEnvironment.GetGame().GetRoomManager().GetRoomsByCategory(result.Id, limit);

                        packet.WriteInteger(getRoomsByCategory.Count);
                        foreach (RoomData data in getRoomsByCategory.ToList()) {
                            RoomAppender.WriteRoom(packet, data, data.Promotion);
                        }

                        break;
                    }

                case NavigatorCategoryType.MyRooms: {
                        ICollection<RoomData> rooms = RoomFactory.GetRoomsDataByOwnerSortByName(habbo.Id).OrderByDescending(x => x.UsersNow).ToList();

                        packet.WriteInteger(rooms.Count);
                        foreach (RoomData data in rooms.ToList()) {
                            RoomAppender.WriteRoom(packet, data, data.Promotion);
                        }

                        break;
                    }

                case NavigatorCategoryType.MyGroups: {
                        List<RoomData> myGroups = new();

                        foreach (Group group in PlusEnvironment.GetGame().GetGroupManager().GetGroupsForUser(habbo.Id).ToList()) {
                            if (group == null)
                                continue;

                            if (!RoomFactory.TryGetData(group.RoomId, out RoomData data))
                                continue;

                            if (!myGroups.Contains(data))
                                myGroups.Add(data);
                        }

                        myGroups = myGroups.Take(limit).ToList();

                        packet.WriteInteger(myGroups.Count);
                        foreach (RoomData data in myGroups.ToList()) {
                            RoomAppender.WriteRoom(packet, data, data.Promotion);
                        }

                        break;
                    }

                case NavigatorCategoryType.MyFriendsRooms: {
                        List<int> roomIds = new();

                        if (habbo == null || habbo.GetMessenger() == null || habbo.GetMessenger().GetFriends() == null)
                            return;

                        foreach (MessengerBuddy buddy in habbo.GetMessenger().GetFriends().Where(p => p.InRoom)) {
                            if (buddy == null || !buddy.InRoom || buddy.UserId == habbo.Id)
                                continue;

                            if (!roomIds.Contains(buddy.CurrentRoom.Id))
                                roomIds.Add(buddy.CurrentRoom.Id);
                        }

                        List<Room> myFriendsRooms = PlusEnvironment.GetGame().GetRoomManager().GetRoomsByIds(roomIds.ToList());

                        packet.WriteInteger(myFriendsRooms.Count);
                        foreach (RoomData data in myFriendsRooms.ToList()) {
                            RoomAppender.WriteRoom(packet, data, data.Promotion);
                        }

                        break;
                    }

                case NavigatorCategoryType.MyRights: {
                        List<RoomData> myRights = new();

                        if (habbo != null) {
                            uint uid = (uint)habbo.Id;
                            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                                List<uint> rightRoomIds = db.RoomRights.Where(r => r.UserId == uid).Take(limit).Select(r => r.RoomId).ToList();

                                foreach (uint roomId in rightRoomIds) {
                                    if (!RoomFactory.TryGetData((int)roomId, out RoomData data))
                                        continue;

                                    if (!myRights.Contains(data))
                                        myRights.Add(data);
                                }
                            }
                        }

                        packet.WriteInteger(myRights.Count);
                        foreach (RoomData data in myRights.ToList()) {
                            RoomAppender.WriteRoom(packet, data, data.Promotion);
                        }

                        break;
                    }

                case NavigatorCategoryType.TopPromotions: {
                        List<Room> getPopularPromotions = PlusEnvironment.GetGame().GetRoomManager().GetOnGoingRoomPromotions(16, limit);

                        packet.WriteInteger(getPopularPromotions.Count);
                        foreach (RoomData data in getPopularPromotions.ToList()) {
                            RoomAppender.WriteRoom(packet, data, data.Promotion);
                        }

                        break;
                    }

                case NavigatorCategoryType.PromotionCategory: {
                        List<Room> getPromotedRooms = PlusEnvironment.GetGame().GetRoomManager().GetPromotedRooms(result.OrderId, limit);

                        packet.WriteInteger(getPromotedRooms.Count);
                        foreach (RoomData data in getPromotedRooms.ToList()) {
                            RoomAppender.WriteRoom(packet, data, data.Promotion);
                        }

                        break;
                    }
            }
        }
    }
}