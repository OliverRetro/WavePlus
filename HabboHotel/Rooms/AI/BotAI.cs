using System;
using System.Drawing;
using System.Linq;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.AI
{
    public abstract class BotAI
    {
        public int BaseId;
        private int _roomId;
        private int _roomUserId;
        private Room _room;
        private RoomUser _roomUser;

        public void Init(int baseId, int roomUserId, int roomId, RoomUser user, Room room)
        {
            BaseId = baseId;
            _roomUserId = roomUserId;
            _roomId = roomId;
            _roomUser = user;
            _room = room;
        }

        public Room GetRoom()
        {
            return _room;
        }

        public RoomUser GetRoomUser()
        {
            return _roomUser;
        }

        public RoomBot GetBotData()
        {
            RoomUser user = GetRoomUser();
            if (user == null)
                return null;

            return GetRoomUser().BotData;
        }

        protected Point? GetBestAdjacentSquare(RoomUser target)
        {
            if (target == null || GetRoom() == null)
                return null;

            Point[] candidates = new[]
            {
                new Point(target.X, target.Y - 1),
                new Point(target.X + 1, target.Y),
                new Point(target.X, target.Y + 1),
                new Point(target.X - 1, target.Y),
                new Point(target.X + 1, target.Y - 1),
                new Point(target.X + 1, target.Y + 1),
                new Point(target.X - 1, target.Y + 1),
                new Point(target.X - 1, target.Y - 1)
            };

            return candidates
                .Where(point => GetRoom().GetGameMap().ValidTile(point.X, point.Y) && GetRoom().GetGameMap().SquareIsOpen(point.X, point.Y, false))
                .OrderBy(point => Math.Abs(point.X - GetRoomUser().X) + Math.Abs(point.Y - GetRoomUser().Y))
                .Cast<Point?>()
                .FirstOrDefault();
        }

        public Point? GetFarAwayWalkableSquare(Point origin, int minimumDistance)
        {
            if (GetRoom() == null)
                return null;

            return Enumerable.Range(0, 40)
                .Select(_ => GetRoom().GetGameMap().GetRandomWalkableSquare())
                .Where(point => point.X > 0 || point.Y > 0)
                .OrderByDescending(point => Math.Abs(point.X - origin.X) + Math.Abs(point.Y - origin.Y))
                .FirstOrDefault(point => Math.Abs(point.X - origin.X) + Math.Abs(point.Y - origin.Y) >= minimumDistance);
        }

        public abstract void OnSelfEnterRoom();
        public abstract void OnSelfLeaveRoom(bool kicked);
        public abstract void OnUserEnterRoom(RoomUser user);
        public abstract void OnUserLeaveRoom(GameClient client);
        public abstract void OnUserSay(RoomUser user, string message);
        public abstract void OnUserShout(RoomUser user, string message);
        public abstract void OnTimerTick();
    }
}