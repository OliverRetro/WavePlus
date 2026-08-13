using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Users.Roleplay;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Roleplay
{
    internal class HealUserCommand : IChatCommand
    {
        public string PermissionRequired => "command_heal_user";
        public string Parameters => "%username%";
        public string Description => "Heal a single player to full HP. Both of you must be in the hospital.";

        public bool UsableWhileDead => true;
        public bool UsableWhileCuffed => true;

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (room?.Group == null) {
                session.SendWhisper("This room is not a corporation.", 1);
                return;
            }

            if (!room.Group.IsOwnerOrHasPermission(session.GetHabbo().Id, "heal_user")) {
                session.SendWhisper("Your group role does not allow you to heal others.", 1);
                return;
            }

            int hospitalRoomId = PlusEnvironment.GetHospitalManager().GetHospitalRoomId();
            if (hospitalRoomId <= 0 || room.RoomId != hospitalRoomId) {
                session.SendWhisper("You can only heal players while in the hospital.", 1);
                return;
            }

            if (@params.Length < 2) {
                session.SendWhisper("Please enter the username of the player you wish to heal.", 1);
                return;
            }

            string username = @params[1];
            RoomUser target = room.GetRoomUserManager().GetRoomUserByHabbo(username);
            if (target == null || target.IsBot || target.GetClient() == null) {
                session.SendWhisper("That player is not in the hospital with you.", 1);
                return;
            }

            Habbo targetHabbo = target.GetClient().GetHabbo();

            if (targetHabbo.CurrentRoomId != hospitalRoomId) {
                session.SendWhisper("That player is not in the hospital.", 1);
                return;
            }

            if (targetHabbo.Id == session.GetHabbo().Id) {
                session.SendWhisper("You cannot heal yourself.", 1);
                return;
            }

            if (!RpCommandUtil.RequireAdjacent(session, room, targetHabbo, "You need to be standing next to the player to heal them."))
                return;

            UserRpStats rpStats = targetHabbo.GetRpStats();

            if (rpStats.Health >= UserRpStats.GetMaxHealth(targetHabbo)) {
                session.SendWhisper(targetHabbo.Username + " is already at full HP.", 1);
                return;
            }

            if (rpStats.IsDead)
                PlusEnvironment.GetHospitalManager().CancelDeath(targetHabbo);

            rpStats.Heal(targetHabbo, 200);

            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId,
                "*heals " + targetHabbo.Username + " back to full health*", 0, 4, isRpAction: true));
        }
    }
}