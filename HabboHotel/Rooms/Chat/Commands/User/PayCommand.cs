using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User
{
    internal class PayCommand : IChatCommand
    {
        public string PermissionRequired => "command_pay";

        public string Parameters => "%username% %amount%";

        public string Description => "Give users credits from your wallet.";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (@params.Length < 3) {
                session.SendWhisper("Usage: :pay <username> <amount>", 1);
                return;
            }

            GameClient target = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(@params[1]);
            Habbo habbo = session?.GetHabbo();
            Habbo targetHabbo = target?.GetHabbo();
            if (habbo == null || targetHabbo == null) {
                session.SendWhisper("Oops, couldn't find that user!", 1);
                return;
            }

            bool valid = int.TryParse(@params[2], out int amount);
            if (!valid || amount < 1) {
                session.SendWhisper("No can do amigo, send an actual amount :)", 1);
                return;
            }

            if (amount > habbo.Credits) {
                session.SendWhisper("You can't afford to send that amount of credits!", 1);
                return;
            }

            // handle creds
            targetHabbo.Credits += amount;
            target.SendPacket(new CreditBalanceComposer(targetHabbo.Credits));

            habbo.Credits -= amount;
            session.SendPacket(new CreditBalanceComposer(habbo.Credits));

            // messages
            target.SendWhisper($"{habbo.Username} sent you {amount} credits!", 1);
            room.SendPacket(new ShoutComposer(room.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id).VirtualId, $"*sends {targetHabbo.Username} a total of {amount} credits*", 0, 4, isRpAction: true));
        }
    }
}