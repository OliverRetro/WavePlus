using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Roleplay.Utilities
{
    public static class RpHomeRoomService
    {
        public static bool SendHome(GameClient session)
        {
            if (session?.GetHabbo() == null)
                return false;

            string raw = PlusEnvironment.GetSettingsManager().TryGetValue("rp.home.room.data");
            if (string.IsNullOrWhiteSpace(raw) || raw == "0")
                return false;

            Habbo habbo = session.GetHabbo();
            habbo.LoadHomeRoomData(raw); // parses the blob + arms PendingReconnectPlacement
            int roomId = habbo.HomeRoomData?.roomid ?? 0;
            if (roomId <= 0)
                return false;

            session.SendPacket(new RoomForwardComposer(roomId));
            return true;
        }
    }
}