using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Police
{
    internal class UncuffCommand : IChatCommand
    {
        public string PermissionRequired => "command_pigs_uncuff";
        public string GroupPermissionRequired => "cuff_user";
        public string Parameters => "%username%";
        public string Description => "Uncuff a user (police only).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2) {
                session.SendWhisper("Usage: :uncuff <username>", 1);
                return;
            }

            if (!RpCommandUtil.TryGetTarget(session, room, @params[1], out GameClient targetClient))
                return;

            PlusEnvironment.GetPoliceManager().Uncuff(targetClient.GetHabbo());
            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*removes " + targetClient.GetHabbo().Username + "' cuffs*", 0, 4, isRpAction: true));
        }
    }
}