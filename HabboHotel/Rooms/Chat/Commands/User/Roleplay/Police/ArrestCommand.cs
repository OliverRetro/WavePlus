using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Utilities;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Police
{
    internal class ArrestCommand : IChatCommand
    {
        public string PermissionRequired => "command_pig_arrest";
        public string GroupPermissionRequired => "arrest_user";
        public string Parameters => "%username%";
        public string Description => "Arrest a cuffed suspect and escort them to jail (police only).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room == null)
                return;

            if (@params.Length < 2) {
                session.SendWhisper("Usage: :arrest <username>", 1);
                return;
            }

            if (!RpCommandUtil.TryGetTarget(session, room, @params[1], out GameClient targetClient))
                return;

            int targetId = targetClient.GetHabbo().Id;

            if (!PlusEnvironment.GetPoliceManager().IsCuffed(targetId)) {
                session.SendWhisper(targetClient.GetHabbo().Username + " must be cuffed before you can arrest them.", 1);
                return;
            }

            if (!RpCommandUtil.RequireAdjacent(session, room, targetClient.GetHabbo(), "You need to be beside the suspect to arrest them."))
                return;

            int stars = PlusEnvironment.GetRpCrimeManager().GetTotalStars(targetId);
            PlusEnvironment.GetRpCrimeManager().ResolveAll(targetId);

            PlusEnvironment.GetPoliceManager().StartArrestEscort(session.GetHabbo(), targetClient.GetHabbo(), stars);

            LiveFeedService.LiveFeed("🚓 <b>" + LiveFeedService.Name(session.GetHabbo().Username, "green") + "</b> arrested <b> " + LiveFeedService.Name(targetClient.GetHabbo().Username) + "</b>");
            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*arrests " + targetClient.GetHabbo().Username + "*", 0, 4, isRpAction: true));
        }
    }
}