using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Database.EF;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;

namespace Plus.Communication.Packets.Incoming.Rooms.FloorPlan
{
    internal class SaveFloorPlanModelEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            if (!session.GetHabbo().InRoom)
                return;

            Room room = session.GetHabbo().CurrentRoom;
            if (room == null || session.GetHabbo().CurrentRoomId != room.Id || !room.CheckRights(session, true))
                return;

            char[] validLetters =
            {
                '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g',
                'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', '\r'
            };

            string map = packet.PopString().ToLower().TrimEnd().Replace("\n", "");

            if (map.Length > 4159) //4096 + New Lines = 4159
            {
                session.SendPacket(new RoomNotificationComposer("floorplan_editor.error", "errors", "(%%%general%%%): %%%too_large_area%%% (%%%max%%% 2048 %%%tiles%%%)"));
                return;
            }

            if (map.Any(letter => !validLetters.Contains(letter)) || string.IsNullOrEmpty(map)) {
                session.SendPacket(new RoomNotificationComposer("floorplan_editor.error", "errors", "Oops, it appears that you have entered an invalid floor map! (Bad characters)"));
                return;
            }

            var modelData = map.Split('\r');

            int sizeY = modelData.Length;
            int sizeX = modelData[0].Length;

            if (sizeY > 64 || sizeX > 64) {
                session.SendPacket(new RoomNotificationComposer("floorplan_editor.error", "errors", "The maximum height and width of a model is 64x64!"));
                return;
            }

            int lastLineLength = 0;
            bool isValid = true;

            foreach (var data in modelData) {
                if (lastLineLength == 0) {
                    lastLineLength = data.Length;
                    continue;
                }

                if (lastLineLength != data.Length) {
                    isValid = false;
                }
            }

            if (!isValid) {
                session.SendPacket(new RoomNotificationComposer("floorplan_editor.error", "errors", "Oops, it appears that you have entered an invalid floor map! (Line length)"));
                return;
            }

            int doorX = packet.PopInt();
            int doorY = packet.PopInt();
            int doorDirection = packet.PopInt();
            int wallThick = packet.PopInt();
            int floorThick = packet.PopInt();
            int wallHeight = packet.RemainingLength() >= 4 ? packet.PopInt() : 0;

            int doorZ = 0;

            try {
                doorZ = Parse(modelData[doorY][doorX]);
            } catch {
                //ignored
            }

            if (wallThick > 1)
                wallThick = 1;

            if (wallThick < -2)
                wallThick = -2;

            if (floorThick > 1)
                floorThick = 1;

            if (floorThick < -2)
                wallThick = -2;

            if (wallHeight < 0)
                wallHeight = 0;

            if (wallHeight > 15)
                wallHeight = 15;

            string modelName = "model_bc_" + room.Id;

            map += '\r' + new string('x', sizeX);

            int rid = room.Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                bool exists = db.RoomModels.Any(m => m.Id == modelName && m.Custom == "1");

                if (!exists) //The row is still null, let's insert instead.
                {
                    // Partial insert (relies on DB defaults for public_items/club_only/poolmap), so raw insert is used.
                    db.Database.ExecuteSqlInterpolated($"INSERT INTO `room_models` (`id`,`door_x`,`door_y`, `door_z`, `door_dir`,`heightmap`,`custom`,`wall_height`) VALUES ({modelName}, {doorX}, {doorY}, {doorZ}, {doorDirection}, {map}, '1', {wallHeight})");
                } else {
                    double doorZd = doorZ;
                    db.RoomModels.Where(m => m.Id == modelName).ExecuteUpdate(s => s
                        .SetProperty(m => m.Heightmap, map)
                        .SetProperty(m => m.DoorX, doorX)
                        .SetProperty(m => m.DoorY, doorY)
                        .SetProperty(m => m.DoorZ, doorZd)
                        .SetProperty(m => m.DoorDir, doorDirection)
                        .SetProperty(m => m.WallHeight, wallHeight));
                }

                db.Rooms.Where(r => r.Id == rid).ExecuteUpdate(s => s
                    .SetProperty(r => r.ModelName, modelName)
                    .SetProperty(r => r.Wallthick, wallThick)
                    .SetProperty(r => r.Floorthick, floorThick));
            }

            room.ModelName = modelName;
            room.WallThickness = wallThick;
            room.FloorThickness = floorThick;

            List<RoomUser> usersToReturn = room.GetRoomUserManager().GetRoomUsers().ToList();

            PlusEnvironment.GetGame().GetRoomManager().ReloadModel(modelName);
            PlusEnvironment.GetGame().GetRoomManager().UnloadRoom(room.Id);

            foreach (RoomUser user in usersToReturn) {
                if (user == null || user.GetClient() == null)
                    continue;

                user.GetClient().SendPacket(new RoomForwardComposer(room.Id));
            }
        }

        private static short Parse(char input)
        {
            switch (input) {
                default:
                    return 0;
                case '1':
                    return 1;
                case '2':
                    return 2;
                case '3':
                    return 3;
                case '4':
                    return 4;
                case '5':
                    return 5;
                case '6':
                    return 6;
                case '7':
                    return 7;
                case '8':
                    return 8;
                case '9':
                    return 9;
                case 'a':
                    return 10;
                case 'b':
                    return 11;
                case 'c':
                    return 12;
                case 'd':
                    return 13;
                case 'e':
                    return 14;
                case 'f':
                    return 15;
                case 'g':
                    return 16;
                case 'h':
                    return 17;
                case 'i':
                    return 18;
                case 'j':
                    return 19;
                case 'k':
                    return 20;
                case 'l':
                    return 21;
                case 'm':
                    return 22;
                case 'n':
                    return 23;
                case 'o':
                    return 24;
                case 'p':
                    return 25;
                case 'q':
                    return 26;
                case 'r':
                    return 27;
                case 's':
                    return 28;
                case 't':
                    return 29;
                case 'u':
                    return 30;
                case 'v':
                    return 31;
                case 'w':
                    return 32;
            }
        }
    }
}