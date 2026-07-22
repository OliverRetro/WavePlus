using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Crime;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay
{
    internal class ChargeCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_charge";

        public string Parameters => "%username% %crime%";

        public string Description => "Charge an adjacent suspect with a named crime, applying its hidden stars (police only).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room == null)
                return;

            if (@params.Length < 3) {
                session.SendWhisper("Usage: :charge <username> <crime>", 1);
                return;
            }

            if (!RpCommandUtil.TryGetTarget(session, room, @params[1], out GameClient targetClient))
                return;

            // Everything after the username is the (possibly multi-word) crime name.
            string crimeQuery = string.Join(" ", @params, 2, @params.Length - 2).Trim();

            RpCrime crime = PlusEnvironment.GetRpCrimeManager().ResolveCrime(crimeQuery);
            if (crime == null) {
                session.SendWhisper("There's no crime called '" + crimeQuery + "'.", 1);
                return;
            }

            if (crime.AutoCharge) {
                session.SendWhisper(crime.Name + " is charged automatically when it's committed — you can only charge crimes that go unreported.", 1);
                return;
            }

            int targetId = targetClient.GetHabbo().Id;
            (bool applied, int stars) = PlusEnvironment.GetRpCrimeManager().ChargeCrime(targetId, crime);

            if (!applied) {
                session.SendWhisper(targetClient.GetHabbo().Username + " has no active " + crime.Name + " charge.", 1);
                return;
            }

            room.SendPacket(new ShoutComposer(
                room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id).VirtualId, "*charges " + targetClient.GetHabbo().Username + " with " + crime.Name + " (" + stars + " stars)*", 0, 4, isRpAction: true));
        }
    }
}