using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorBank : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
        }

        public void OnRemove(GameClient session, Item item)
        {
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            Habbo habbo = session?.GetHabbo();
            if (habbo == null)
                return;

            if (habbo.GetBankAccount() != null) {
                PlusEnvironment.GetBankingManager().SendBankSnapshot(habbo);
                return;
            }

            if (!PlusEnvironment.GetBankingManager().CreateAccount(habbo)) {
                session.SendWhisper("You couldn't open an account — you may not have enough credits for the opening fee.", 1);
                return;
            }

            session.SendWhisper("Your bank account has been opened! Use an ATM to deposit and withdraw.", 1);
            PlusEnvironment.GetBankingManager().SendBankSnapshot(habbo);
        }

        public void OnWiredTrigger(Item item)
        {
        }
    }
}