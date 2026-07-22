using Plus.Communication.Packets.Outgoing.Roleplay;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorAtm : IFurniInteractor
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
            if (habbo == null || item?.GetRoom() == null)
                return;

            // Must be standing on or next to the ATM (<= 1 tile, diagonals included).
            RoomUser user = item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (user == null || !Gamemap.TilesTouching(item.GetX, item.GetY, user.X, user.Y))
                return;

            if (habbo.GetBankAccount() == null) {
                session.SendWhisper("Sorry, you don't have a bank account, visit the bank to open one!", 1);
                return;
            }

            // Publish the balance to the purse (currency 200) then tell the client to open the ATM.
            PlusEnvironment.GetBankingManager().SendBankCurrency(habbo);

            // ATM view shows the ATM-specific fees (falls back to .default) for both directions.
            int withdrawFeePercent = Plus.HabboHotel.Roleplay.Banking.BankingManager.ResolveFeePercent("withdrawal", "ATM");
            int depositFeePercent = Plus.HabboHotel.Roleplay.Banking.BankingManager.ResolveFeePercent("deposit", "ATM");

            session.SendPacket(new OpenAtmComposer(withdrawFeePercent, depositFeePercent));
        }

        public void OnWiredTrigger(Item item)
        {
        }
    }
}