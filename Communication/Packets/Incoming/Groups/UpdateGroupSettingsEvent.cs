using System.Linq;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Rooms.Permissions;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Groups
{
    internal class UpdateGroupSettingsEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            int groupId = packet.PopInt();

            if (!PlusEnvironment.GetGame().GetGroupManager().TryGetGroup(groupId, out Group group))
                return;

            if (group.CreatorId != session.GetHabbo().Id)
                return;

            int type = packet.PopInt();
            int furniOptions = packet.PopInt();

            switch (type)
            {
                default:
                    group.Type = GroupType.Open;
                    break;
                case 1:
                    group.Type = GroupType.Locked;
                    break;
                case 2:
                    group.Type = GroupType.Private;
                    break;
            }

            if (group.Type != GroupType.Locked)
            {
                if (group.GetRequests.Count > 0)
                {
                    foreach (int userId in group.GetRequests.ToList())
                    {
                        group.HandleRequest(userId, false);
                    }

                    group.ClearRequests();
                }
            }

            group.AdminOnlyDeco = furniOptions;
            group.MarkDirty(); // state + admindeco cached; written to DB on the 15-minute group flush

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(group.RoomId, out Room room))
                return;

            foreach (RoomUser user in room.GetRoomUserManager().GetRoomUsers().ToList())
            {
                if (room.OwnerId == user.UserId || group.IsAdmin(user.UserId) || !group.IsMember(user.UserId))
                    continue;

                if (furniOptions == 1)
                {
                    user.RemoveStatus("flatctrl 1");
                    user.UpdateNeeded = true;

                    user.GetClient().SendPacket(new YouAreControllerComposer(0));
                }
                else if (furniOptions == 0 && !user.Statusses.ContainsKey("flatctrl 1"))
                {
                    user.SetStatus("flatctrl 1");
                    user.UpdateNeeded = true;

                    user.GetClient().SendPacket(new YouAreControllerComposer(1));
                }
            }

            session.SendPacket(new GroupInfoComposer(group, session));
        }
    }
}