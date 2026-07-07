using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Roleplay.Trash;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorTrashBin : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
            if (item != null)
                item.ExtraData = "0";
        }

        public void OnRemove(GameClient session, Item item)
        {
            if (item != null)
                item.ExtraData = "0";
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            PlusEnvironment.GetTrashSearchManager()?.StartSearch(session, item);
        }

        public void OnWiredTrigger(Item item)
        {
        }
    }
}
