using System;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.TargetLock;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Logs;
using Plus.HabboHotel.Users;
using Plus.Utilities;

namespace Plus.Communication.Packets.Incoming.Rooms.Chat
{
    internal static class RpMentionWhisper
    {
        private const string TargetPlaceholder = "x";

        public static bool TryHandle(GameClient session, Room room, RoomUser user, string message, int colour)
        {
            if (session?.GetHabbo() == null || room == null || user == null)
                return false;

            if (string.IsNullOrEmpty(message) || message[0] != '@')
                return false;

            int space = message.IndexOf(' ');

            if (space < 1)
                return false;

            string handle = message[1..space].Trim();
            string body = message[(space + 1)..].Trim();

            if (handle.Length == 0 || body.Length == 0)
                return false;

            Habbo target = ResolveTarget(session, handle);

            if (target == null)
            {
                session.SendWhisper(
                    string.Equals(handle, TargetPlaceholder, StringComparison.OrdinalIgnoreCase)
                        ? "You aren't targeting anyone - click a player or lock a target first."
                        : "Couldn't find \"" + handle + "\" to whisper.", 1);
                return true;
            }

            if (target.Id == session.GetHabbo().Id)
            {
                session.SendWhisper("You can't whisper yourself.", 1);
                return true;
            }

            Room targetRoom = target.CurrentRoom;
            RoomUser targetUser = targetRoom?.GetRoomUserManager().GetRoomUserByHabbo(target.Id);

            if (targetUser == null || targetUser.IsBot || targetUser.GetClient()?.GetHabbo() == null)
            {
                session.SendWhisper(target.Username + " isn't in a room right now.", 1);
                return true;
            }

            Habbo targetHabbo = targetUser.GetClient().GetHabbo();

            if (!targetHabbo.ReceiveWhispers && !session.GetHabbo().GetPermissions().HasRight("room_whisper_override"))
            {
                session.SendWhisper("Oops, this user has their whispers disabled!", 1);
                return true;
            }

            if (!session.GetHabbo().GetPermissions().HasRight("word_filter_override"))
                body = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(body);

            int bubble = session.GetHabbo().CustomBubbleId == 0 ? colour : session.GetHabbo().CustomBubbleId;
            user.LastBubble = bubble;

            PlusEnvironment.GetGame().GetChatManager().GetLogs().StoreChatLog(
                new ChatLogEntry(session.GetHabbo().Id, room.Id, "<Mention to " + target.Username + ">: " + body,
                    UnixTimestamp.GetNow(), session.GetHabbo(), room));

            session.SendPacket(new WhisperComposer(user.VirtualId, body, 0, bubble));

            if (targetHabbo.GetIgnores().IgnoredUserIds().Contains(session.GetHabbo().Id))
                return true;

            bool sameRoom = targetRoom.Id == room.Id;
            int bubbleOwner = sameRoom ? user.VirtualId : targetUser.VirtualId;

            targetUser.GetClient().SendPacket(
                new MentionWhisperComposer(bubbleOwner, "From: " + session.GetHabbo().Username + ": " + body, bubble));

            return true;
        }

        private static Habbo ResolveTarget(GameClient session, string handle)
        {
            if (string.Equals(handle, TargetPlaceholder, StringComparison.OrdinalIgnoreCase))
                return TargetLockService.GetTarget(session.GetHabbo());

            return PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(handle)?.GetHabbo();
        }
    }
}
