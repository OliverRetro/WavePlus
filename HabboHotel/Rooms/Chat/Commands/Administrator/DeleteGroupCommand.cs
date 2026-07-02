using Plus.Database.Interfaces;
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

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `groups` WHERE `id` = '" + room.Group.Id + "'");
                dbClient.RunQuery("DELETE FROM `group_memberships` WHERE `group_id` = '" + room.Group.Id + "'");
                dbClient.RunQuery("DELETE FROM `group_roles` WHERE `group_id` = '" + room.Group.Id + "'");
                dbClient.RunQuery("DELETE FROM `group_requests` WHERE `group_id` = '" + room.Group.Id + "'");
                dbClient.RunQuery("UPDATE `rooms` SET `group_id` = '0' WHERE `group_id` = '" + room.Group.Id + "' LIMIT 1");
                dbClient.RunQuery("UPDATE `user_stats` SET `groupid` = '0' WHERE `groupid` = '" + room.Group.Id + "' LIMIT 1");
                dbClient.RunQuery("DELETE FROM `items_groups` WHERE `group_id` = '" + room.Group.Id + "'");
            }

            PlusEnvironment.GetGame().GetGroupManager().DeleteGroup(room.Group.Id);

            room.Group = null;

            PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(room.Id);

            session.SendNotification("Success, group deleted.");
        }
    }
}
