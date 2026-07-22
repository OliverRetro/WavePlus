using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Groups
{
    internal class DeleteGroupEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(packet.PopInt(), out Group group)) {
                session.SendNotification("Oops, we couldn't find that group!");
                return;
            }

            if (group.CreatorId != session.GetHabbo().Id && !session.GetHabbo().GetPermissions().HasRight("group_delete_override")) //Maybe a FUSE check for staff override?
            {
                session.SendNotification("Oops, only the group owner can delete a group!");
                return;
            }

            if (group.MemberCount >= Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("group.delete.member.limit")) && !session.GetHabbo().GetPermissions().HasRight("group_delete_limit_override")) {
                session.SendNotification("Oops, your group exceeds the maximum amount of members (" + Convert.ToInt32(PlusEnvironment.GetSettingsManager().TryGetValue("group.delete.member.limit")) + ") a group can exceed before being eligible for deletion. Seek assistance from a staff member.");
                return;
            }

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(group.RoomId, out Room room))
                return;

            if (!RoomFactory.TryGetData(group.RoomId, out RoomData _))
                return;

            room.Group = null;

            //Remove it from the cache.
            PlusEnvironment.GetGame().GetGroupManager().DeleteGroup(group.Id);

            //Now the :S stuff.
            int groupIdInt = group.Id;
            uint groupIdUint = (uint)group.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Groups.Where(x => x.Id == groupIdUint).ExecuteDelete();
                db.GroupMemberships.Where(x => x.GroupId == groupIdUint).ExecuteDelete();
                db.GroupRoles.Where(x => x.GroupId == groupIdInt).ExecuteDelete();
                db.GroupRequests.Where(x => x.GroupId == groupIdUint).ExecuteDelete();
                db.Rooms.Where(x => x.GroupId == groupIdUint).ExecuteUpdate(s => s.SetProperty(x => x.GroupId, 0u));
                db.UserStats.Where(x => x.Groupid == groupIdInt).ExecuteUpdate(s => s.SetProperty(x => x.Groupid, 0));
                db.ItemsGroups.Where(x => x.GroupId == groupIdInt).ExecuteDelete();
            }

            //Unload it last.
            PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(room.Id);

            //Say hey!
            session.SendNotification("You have successfully deleted your group.");
        }
    }
}