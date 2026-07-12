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
            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                db.groups.Where(x => x.id == groupIdUint).ExecuteDelete();
                db.group_memberships.Where(x => x.group_id == groupIdUint).ExecuteDelete();
                db.group_roles.Where(x => x.group_id == groupIdInt).ExecuteDelete();
                db.group_requests.Where(x => x.group_id == groupIdUint).ExecuteDelete();
                db.rooms.Where(x => x.group_id == groupIdUint).ExecuteUpdate(s => s.SetProperty(x => x.group_id, 0u));
                db.user_stats.Where(x => x.groupid == groupIdInt).ExecuteUpdate(s => s.SetProperty(x => x.groupid, 0));
                db.items_groups.Where(x => x.group_id == groupIdInt).ExecuteDelete();
            }

            PlusEnvironment.GetGame().GetGroupManager().DeleteGroup(room.Group.Id);

            room.Group = null;

            PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(room.Id);

            session.SendNotification("Success, group deleted.");
        }
    }
}
