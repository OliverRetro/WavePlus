using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Corporation
{
    internal static class GroupHierarchy
    {
        public const string OverrideRight = "any_group_owner";

        public static bool HasOverride(GameClient session, Group group) =>
            session?.GetHabbo() != null
            && group != null
            && (session.GetHabbo().Id == group.CreatorId || session.GetHabbo().GetPermissions().HasRight(OverrideRight));

        public static bool CanActOn(GameClient session, Group group, int targetId)
        {
            if (HasOverride(session, group))
                return true;

            int actorId = session.GetHabbo().Id;

            return group.GetRank(actorId) > group.GetRank(targetId);
        }

        public static bool CanAssignLevel(GameClient session, Group group, int level)
        {
            if (HasOverride(session, group))
                return true;

            int actorRank = group.GetRank(session.GetHabbo().Id);

            // Level 0 is the owner slot and ranks as int.MaxValue; nobody but an
            // override holder may ever assign it.
            if (level <= 0)
                return false;

            return actorRank > level;
        }
    }
}