using System;
using Plus.Communication.Packets.Outgoing.Overlay;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.TargetLock;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.Communication.Packets.Incoming.Rooms.Chat
{
    internal static class RpMention
    {
        private const string TargetPlaceholder = "x";

        public static void Handle(GameClient session, Room room, RoomUser user, string message)
        {
            if (session?.GetHabbo() == null || room == null || user == null)
                return;

            if (string.IsNullOrEmpty(message) || message[0] != '@')
                return;

            int space = message.IndexOf(' ');

            if (space < 1)
                return;

            string handle = message[1..space].Trim();
            string body = message[(space + 1)..].Trim();

            if (handle.Length == 0 || body.Length == 0)
                return;

            Habbo target = ResolveTarget(session, handle);

            if (target == null || target.Id == session.GetHabbo().Id)
                return;

            RoomUser targetUser = target.CurrentRoom?.GetRoomUserManager().GetRoomUserByHabbo(target.Id);
            if (targetUser == null || targetUser.IsBot || targetUser.GetClient()?.GetHabbo() == null)
                return;

            Habbo targetHabbo = targetUser.GetClient().GetHabbo();

            if (targetHabbo.GetIgnores().IgnoredUserIds().Contains(session.GetHabbo().Id))
                return;

            string notice = body;
            if (!session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
                notice = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(notice);

            WebOverlay.SendMention(targetUser.GetClient(), session.GetHabbo().Username, notice);
        }

        private static Habbo ResolveTarget(GameClient session, string handle)
        {
            if (string.Equals(handle, TargetPlaceholder, StringComparison.OrdinalIgnoreCase))
                return TargetLockService.GetTarget(session.GetHabbo());

            return PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(handle)?.GetHabbo();
        }
    }
}