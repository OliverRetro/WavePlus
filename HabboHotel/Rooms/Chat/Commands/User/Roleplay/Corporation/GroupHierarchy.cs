using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Corporation
{
    internal static class GroupHierarchy
    {
        public const string OverrideRight = "any_group_owner";

        public static bool CanActOn(GameClient session, Group group, int targetId)
        {
            int actorId = session.GetHabbo().Id;
            if (actorId == group.CreatorId || session.GetHabbo().GetPermissions().HasRight(OverrideRight))
                return true;

            return !group.TargetOutranks(actorId, targetId);
        }
    }
}
