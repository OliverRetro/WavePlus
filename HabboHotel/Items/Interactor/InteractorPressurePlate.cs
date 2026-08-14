using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Items.Interactor
{
    /// <summary>
    /// A pressure plate only changes state from someone standing on it, so every manual trigger
    /// path — double click and wired — is deliberately inert. See Item.PressPressurePlate.
    /// </summary>
    public class InteractorPressurePlate : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
            // Dropping a plate under someone who is already standing there still counts as pressed.
            item.RefreshPressurePlate();
        }

        public void OnRemove(GameClient session, Item item)
        {
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
        }

        public void OnWiredTrigger(Item item)
        {
        }
    }
}