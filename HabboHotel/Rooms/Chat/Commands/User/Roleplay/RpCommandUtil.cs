using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Utilities;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay
{
    internal static class RpCommandUtil
    {
        public static bool TryGetTarget(GameClient session, Room room, string username, out GameClient targetClient)
        {
            targetClient = null;

            if (session?.GetHabbo() == null || room == null || string.IsNullOrWhiteSpace(username)) {
                session?.SendWhisper("Please enter a username.", 1);
                return false;
            }

            targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(username);
            if (targetClient?.GetHabbo() == null) {
                session.SendWhisper("That user could not be found.", 1);
                return false;
            }

            if (targetClient == session) {
                session.SendWhisper("You cannot do that to yourself.", 1);
                return false;
            }

            if (targetClient.GetHabbo().CurrentRoomId != session.GetHabbo().CurrentRoomId) {
                session.SendWhisper("That user is not in the same room as you.", 1);
                return false;
            }

            return true;
        }

        public static bool AreAdjacent(Room room, Habbo a, Habbo b)
        {
            RoomUser ua = room?.GetRoomUserManager().GetRoomUserByHabbo(a.Id);
            RoomUser ub = room?.GetRoomUserManager().GetRoomUserByHabbo(b.Id);
            if (ua == null || ub == null)
                return false;

            // Shares combat's reach rules, so cuffing/arresting a diagonally adjacent suspect
            // works exactly like swinging at them does — including while either party is walking.
            return RpProximity.IsWithinWeaponRange(ua, ub, 1, allowDiagonal: true);
        }

        public static bool RequireAdjacent(GameClient session, Room room, Habbo target, string message)
        {
            if (AreAdjacent(room, session.GetHabbo(), target))
                return true;

            session.SendWhisper(message, 1);
            return false;
        }
    }
}