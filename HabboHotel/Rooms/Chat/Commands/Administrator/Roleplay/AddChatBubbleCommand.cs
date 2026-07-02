using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.GameClients;
using System;
using System.Collections.Generic;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class AddChatBubbleCommand : IChatCommand
    {
        public string PermissionRequired => "command_add_chat_bubble";

        public string Parameters => "%username% %id%";

        public string Description => "Give user a chat bubble.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 3) {
                session.SendWhisper("Please enter a username and a chat bubble ID. Example usage is :chatbubble Oliver 5.");
                return;
            }

            if (!int.TryParse(@params[2], out int bubbleId)) {
                session.SendWhisper("Please enter a valid chat bubble ID.");
                return;
            }

            if (!PlusEnvironment.GetGame().GetChatManager().GetChatStyles().TryGetStyle(bubbleId, out ChatStyle style)) {
                session.SendWhisper("That chat bubble does not exist.");
                return;
            }

            GameClient targetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            if (targetClient == null) {
                session.SendWhisper("An error occurred whilst finding that user, maybe they're not online.");
                return;
            }

            if (targetClient.GetHabbo() == null) {
                session.SendWhisper("An error occurred whilst finding that user, maybe they're not online.");
                return;
            }

            if (style.RequiredRight.Length > 0 && !targetClient.GetHabbo().GetPermissions().HasRight(style.RequiredRight)) {
                session.SendWhisper(targetClient.GetHabbo().Username + " does not have the required right for that bubble.");
                return;
            }

            if (targetClient.GetHabbo().HasChatBubble(bubbleId)) {
                session.SendWhisper(targetClient.GetHabbo().Username + " already owns that chat bubble.");
                return;
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `user_chat_bubbles` (`chat_bubble_id`, `user_id`, `created_at`) VALUES (@bubbleId, @userId, NOW())");
                dbClient.AddParameter("bubbleId", bubbleId);
                dbClient.AddParameter("userId", targetClient.GetHabbo().Id);
                dbClient.RunQuery();
            }

            targetClient.GetHabbo().OwnedChatBubbleIds.Add(bubbleId);
            targetClient.SendPacket(new BubbleAlertComposer("BUBBLE", new Dictionary<string, string>
            {
                ["message"] = "You've unlocked a new chat bubble!",
                ["image"] = "chatbubble/bubble_" + bubbleId + ".png"
            }));

            session.SendWhisper("Chat bubble given to " + targetClient.GetHabbo().Username);
        }
    }
}
