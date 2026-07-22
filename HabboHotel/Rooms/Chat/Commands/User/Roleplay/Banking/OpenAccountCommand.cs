using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.User.Roleplay.Banking
{
    internal class OpenAccountCommand : IChatCommand
    {
        public string PermissionRequired => "command_rp_bank_openaccount";

        public string Parameters => "";

        public string Description => "Open a bank account (must be done in a bank room).";

        public void Execute(GameClient session, Room room, string[] @params)
        {
            if (session?.GetHabbo() == null || room == null)
                return;

            if (session.GetHabbo().GetBankAccount() != null) {
                session.SendWhisper("You already have a bank account.", 1);
                return;
            }

            if (!IsBankRoom(room.Id)) {
                session.SendWhisper("You can only open a bank account inside the bank.", 1);
                return;
            }

            if (!PlusEnvironment.GetBankingManager().CreateAccount(session.GetHabbo())) {
                session.SendWhisper("You couldn't open an account — you may not have enough credits for the opening fee.", 1);
                return;
            }

            BankCommandUtil.Shout(session, room,
                "*opens a bank account*");
            session.SendWhisper("Your bank account has been opened! Use :balance, :deposit and :withdraw at an ATM.", 1);
        }

        private static bool IsBankRoom(int roomId)
        {
            string raw = PlusEnvironment.GetSettingsManager().TryGetValue("rp.bank.rooms");
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            foreach (string part in raw.Split(';'))
                if (int.TryParse(part.Trim(), out int id) && id == roomId)
                    return true;

            return false;
        }
    }
}