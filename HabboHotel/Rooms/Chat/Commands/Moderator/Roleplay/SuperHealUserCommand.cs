using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Roleplay;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Roleplay
{
    internal class SuperHealUserCommand : IChatCommand
    {
        public string PermissionRequired => "command_super_heal";
        public string Parameters => "%username%";
        public string Description => "Heal a single player to full HP.";

        public bool UsableWhileDead => true;
        public bool UsableWhileCuffed => true;

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2) {
                session.SendWhisper("Please enter the username of the player you wish to heal.", 1);
                return;
            }

            string username = @params[1];
            RoomUser target = room.GetRoomUserManager().GetRoomUserByHabbo(username);
            if (target == null || target.IsBot || target.GetClient() == null) {
                session.SendWhisper("Couldn't find the user.", 1);
                return;
            }

            Habbo targetHabbo = target.GetClient().GetHabbo();
            UserRpStats rpStats = targetHabbo.GetRpStats();

            if (rpStats.IsDead)
                PlusEnvironment.GetHospitalManager().CancelDeath(targetHabbo);

            rpStats.Heal(targetHabbo, 200);

            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId,
                "*super heals " + targetHabbo.Username + " back to full health*", 0, 23, isRpAction: true));
        }
    }
}