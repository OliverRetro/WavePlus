using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class MayhemCommand : IChatCommand
    {
        public string PermissionRequired => "command_mayhem";

        public string Parameters => "";

        public string Description => "Unleash 60 seconds of Teletubby mayhem — bots swarm from the arrows and attack everyone. Run again to stop it early.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room == null)
                return;

            if (PlusEnvironment.GetMayhemManager().IsActive(room.Id)) {
                PlusEnvironment.GetMayhemManager().Stop(room.Id);
                session.SendWhisper("Mayhem stopped — the bots are fleeing.", 1);
                return;
            }

            if (!PlusEnvironment.GetMayhemManager().TryStart(room, out string error)) {
                session.SendWhisper(error, 1);
                return;
            }

            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*unleashes the relentless death of a thousand Teletubbies*", 0, 23, isRpAction: true));
            session.SendWhisper("Run :mayhem again to stop it.", 1);
        }
    }
}