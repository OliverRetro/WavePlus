using System;
using Plus.HabboHotel.Bots;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay
{
    internal class TaxiCommand : IChatCommand
    {
        public string PermissionRequired => string.Empty;

        public string Parameters => "%targetroomid%";

        public string Description => "Call a taxi to another serviced room.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2 || !int.TryParse(@params[1], out int targetRoomId) || targetRoomId <= 0)
            {
                session.SendWhisper("Usage: :taxi <targetroomid>");
                return;
            }

            RoomUser roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (roomUser == null)
                return;

            if (targetRoomId == room.Id)
            {
                session.SendWhisper("You cannot take a taxi to the room you are already in.");
                return;
            }

            if (!BotManager.IsNearTaxiPole(room, roomUser))
            {
                session.SendWhisper("You need to be near a telephone or own one to call a taxi!");
                return;
            }

            if (!PlusEnvironment.GetGame().GetBotManager().TryCallTaxi(session, targetRoomId, out string error))
            {
                session.SendWhisper(error);
                return;
            }

            if (PlusEnvironment.GetGame().GetBotManager().TryGetTaxiPoints(room.Id, out System.Collections.Generic.List<TaxiPointData> points) && points.Count > 0)
                session.SendWhisper("Your taxi will arrive in approximately " + points[0].Delay + " seconds!");
            else
                session.SendWhisper("Your taxi is on the way.");
        }
    }
}
