using System;
using Plus.HabboHotel.Bots;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class SetTaxiPointCommand : IChatCommand
    {
        public string PermissionRequired => "command_room";

        public string Parameters => "%cost% %delay%";

        public string Description => "Set the taxi pickup point for this room.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (!room.CheckRights(session, true)) {
                session.SendWhisper("Oops, only the room owner or staff can use this command.");
                return;
            }

            if (@params.Length < 3 || !int.TryParse(@params[1], out int cost) || !int.TryParse(@params[2], out int delay)) {
                session.SendWhisper("Usage: :settaxipoint <cost> <delay>");
                return;
            }

            if (cost < 0 || delay < 0) {
                session.SendWhisper("Taxi cost and delay must both be 0 or higher.");
                return;
            }

            RoomUser roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (roomUser == null)
                return;

            PlusEnvironment.GetGame().GetBotManager().AddTaxiPoint(new TaxiPointData(room.Id, roomUser.X, roomUser.Y, roomUser.Z, roomUser.RotBody, cost, delay));
            session.SendWhisper("Taxi point added.");
        }
    }
}
