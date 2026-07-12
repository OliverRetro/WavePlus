using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Roleplay.Utilities
{
    /// <summary>
    /// Forwards a user to the server-configured RP "home" room whenever they are booted out of a
    /// room (room unload, room deletion, kick, or room ban). The destination is the
    /// <c>rp.home.room.data</c> server setting, stored as a <c>HomeRoomDataState</c> JSON blob
    /// ({roomid,x,y,z,rotation,weapon}); seeding it via <see cref="Habbo.LoadHomeRoomData"/> arms
    /// the reconnect placement so <see cref="Habbo.ApplyHomeRoomData"/> positions them (and clears
    /// the weapon effect when weapon == 0) on entry.
    /// </summary>
    public static class RpHomeRoomService
    {
        /// <summary>
        /// Forwards <paramref name="session"/> to the configured RP home room. Returns false when no
        /// valid home room is configured, so callers can fall back to the hotel view.
        /// </summary>
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
