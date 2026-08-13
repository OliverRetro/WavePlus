using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorRubbish : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
        }

        public void OnRemove(GameClient session, Item item)
        {
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            PlusEnvironment.GetRubbishManager()?.TryPickup(session, item);
        }

        public void OnWiredTrigger(Item item)
        {
        }
    }
}