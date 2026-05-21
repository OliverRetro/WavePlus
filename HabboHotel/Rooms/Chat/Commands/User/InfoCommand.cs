using System;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class InfoCommand : IChatCommand
    {
        public string PermissionRequired => "command_info";

        public string Parameters => "";

        public string Description => "Displays generic information that everybody loves to see.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            TimeSpan uptime = DateTime.Now - PlusEnvironment.ServerStarted;
            int onlineUsers = PlusEnvironment.GetGame().GetClientManager().Count;
            int roomCount = PlusEnvironment.GetGame().GetRoomManager().Count;

            session.SendPacket(new RoomNotificationComposer("Powered by Plus Emulator",
                "<b>Current run time information</b>:\n" +
                "Online Users: " + onlineUsers + "\n" +
                "Rooms Loaded: " + roomCount + "\n" +
                "Uptime: " + uptime.Days + " day(s), " + uptime.Hours + " hours and " + uptime.Minutes + " minutes.\n\n", "plus", ""));
        }
    }
}