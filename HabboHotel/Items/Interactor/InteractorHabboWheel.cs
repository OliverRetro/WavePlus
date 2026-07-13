using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorHabboWheel : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
            item.ExtraData = "-1";
            item.RequestUpdate(10, true);
        }

        public void OnRemove(GameClient session, Item item)
        {
            item.ExtraData = "-1";
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            if (!hasRights)
            {
                return;
            }

            if (item.ExtraData != "-1")
            {
                item.ExtraData = "-1";
                item.UpdateState();
                item.RequestUpdate(10, true);
            }
        }

        public void OnWiredTrigger(Item item)
        {
            if (item.ExtraData != "-1")
            {
                item.ExtraData = "-1";
                item.UpdateState();
                item.RequestUpdate(10, true);
            }
        }
    }
}