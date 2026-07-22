using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator
{
    internal class DeleteGroupCommand : IChatCommand
    {
        public string PermissionRequired => "command_delete_group";

        public string Parameters => "";

        public string Description => "Delete a group from the database and cache.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            room = session.GetHabbo().CurrentRoom;
            if (room == null)
                return;

            if (room.Group == null) {
                session.SendWhisper("Oops, there is no group here?");
                return;
            }

            int groupIdInt = room.Group.Id;
            uint groupIdUint = (uint)room.Group.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Groups.Where(x => x.Id == groupIdUint).ExecuteDelete();
                db.GroupMemberships.Where(x => x.GroupId == groupIdUint).ExecuteDelete();
                db.GroupRoles.Where(x => x.GroupId == groupIdInt).ExecuteDelete();
                db.GroupRequests.Where(x => x.GroupId == groupIdUint).ExecuteDelete();
                db.Rooms.Where(x => x.GroupId == groupIdUint).ExecuteUpdate(s => s.SetProperty(x => x.GroupId, 0u));
                db.UserStats.Where(x => x.Groupid == groupIdInt).ExecuteUpdate(s => s.SetProperty(x => x.Groupid, 0));
                db.ItemsGroups.Where(x => x.GroupId == groupIdInt).ExecuteDelete();
            }

            PlusEnvironment.GetGame().GetGroupManager().DeleteGroup(room.Group.Id);

            room.Group = null;

            PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(room.Id);

            session.SendNotification("Success, group deleted.");
        }
    }
}