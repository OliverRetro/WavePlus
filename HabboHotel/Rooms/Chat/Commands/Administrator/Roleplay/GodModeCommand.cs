using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Administrator.Roleplay
{
    internal class GodModeCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_godmode";

        public string Parameters => "";

        public string Description => "You know it g.";

        // Staff recovery tool — has to work from the grave and in cuffs, or there's no way out.
        public bool UsableWhileDead => true;

        public bool UsableWhileCuffed => true;

        public void Execute(GameClient session, Room room, string[] @params)
        {
            var stats = session.GetHabbo().GetRpStats();
            if (stats == null || session.GetHabbo() == null) { return; }
            stats.IsGodMode = !stats.IsGodMode;

            bool god = stats.IsGodMode;
            RoomUser roomUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            room.SendPacket(new ShoutComposer(roomUser.VirtualId, god ? "*enters God mode*" : "*exits God mode*", 0, 11, isRpAction: true));

            if (stats.IsGodMode) {
                roomUser.ApplyEffect(59);
            } else {
                roomUser.ApplyEffect(0);
            }
        }
    }
}