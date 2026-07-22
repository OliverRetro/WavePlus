using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Police
{
    internal class PardonCommand : IChatCommand
    {
        public string PermissionRequired => "command_pigs_pardon";
        public string GroupPermissionRequired => "pardon_user";
        public string Parameters => "%username%";
        public string Description => "Clear a user's active wanted crimes (police only).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 2) {
                session.SendWhisper("Usage: :pardon <username>", 1);
                return;
            }

            if (!RpCommandUtil.TryGetTarget(session, room, @params[1], out GameClient targetClient))
                return;

            PlusEnvironment.GetRpCrimeManager().ResolveAll(targetClient.GetHabbo().Id);
            PlusEnvironment.GetJailManager().Pardon(targetClient.GetHabbo());

            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*pardons " + targetClient.GetHabbo().Username + " for their crimes*", 0, 4, isRpAction: true));
        }
    }
}