using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class Pathfinding3DCommand : IChatCommand
    {
        public string PermissionRequired => "command_pathfinding_3d";

        public string Parameters => "[on/off]";

        public string Description => "Toggle 3D pathfinding (walking under furniture) for this room.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (!room.CheckRights(session, true)) {
                session.SendWhisper("Oops, only the owner of this room can run this command!");
                return;
            }

            bool enabled = @params.Length > 1
                ? @params[1].ToLower() is "on" or "1" or "true" or "yes"
                : !room.Pathfinding3D;

            if (enabled == room.Pathfinding3D) {
                session.SendWhisper("3D pathfinding is already " + (enabled ? "on" : "off") + " in this room.");
                return;
            }

            room.Pathfinding3D = enabled;

            int roomId = room.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.Rooms.Where(r => r.Id == roomId)
                    .ExecuteUpdate(s => s.SetProperty(r => r.Pathfinding3d, enabled));
            }

            session.SendWhisper(enabled
                ? "3D pathfinding is now ON — routes may pass under furniture and over what others walk beneath."
                : "3D pathfinding is now OFF — every tile offers its top surface only, which is the cheaper search.");
        }
    }
}