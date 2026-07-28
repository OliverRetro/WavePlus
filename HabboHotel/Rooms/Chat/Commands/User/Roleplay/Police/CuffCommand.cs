using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.RpItem.Weapon;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Police
{
    internal class CuffCommand : IChatCommand
    {
        public string PermissionRequired => "command_pigs_cuff";
        public string GroupPermissionRequired => "cuff_user";
        public string Parameters => "%username%";
        public string Description => "Cuff a charged, adjacent suspect (police only).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2) {
                session.SendWhisper("Usage: :cuff <username>", 1);
                return;
            }

            if (!RpCommandUtil.TryGetTarget(session, room, @params[1], out GameClient targetClient))
                return;

            Habbo target = targetClient.GetHabbo();

            if (!PlusEnvironment.GetRpCrimeManager().IsCharged(target.Id)) {
                session.SendWhisper(target.Username + " hasn't been charged yet — use :charge first.", 1);
                return;
            }

            if (PlusEnvironment.GetPoliceManager().IsCuffed(target.Id)) {
                session.SendWhisper(target.Username + " is already cuffed.", 1);
                return;
            }

            if (!RpCommandUtil.RequireAdjacent(session, room, target, "You need to be beside the suspect to cuff them."))
                return;

            RpWeaponService.Disarm(target);

            if (PlusEnvironment.GetGame().GetShiftManager().IsWorking(target.Id))
                PlusEnvironment.GetGame().GetShiftManager().InterruptShift(target, target.CurrentRoom);

            PlusEnvironment.GetPoliceManager().Cuff(session.GetHabbo(), target);
            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*cuffs " + targetClient.GetHabbo().Username + "*", 0, 4, isRpAction: true));
        }
    }
}