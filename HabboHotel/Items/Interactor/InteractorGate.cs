using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items.Wired;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorGate : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
        }

        public void OnRemove(GameClient session, Item item)
        {
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            int modes = item.GetBaseItem().Modes - 1;

            if (!hasRights)
            {
                return;
            }

            if (modes <= 0)
            {
                item.UpdateState(false, true);
            }

            int currentMode = 0;
            int newMode = 0;

            if (!int.TryParse(item.ExtraData, out currentMode))
            {
            }

            if (currentMode <= 0)
            {
                newMode = 1;
            }
            else if (currentMode >= modes)
            {
                newMode = 0;
            }
            else
            {
                newMode = currentMode + 1;
            }

            if (newMode == 0)
            {
                if (!item.GetRoom().GetGameMap().ItemCanBePlaced(item.GetX, item.GetY))
                {
                    return;
                }
            }

            item.ExtraData = newMode.ToString();
            item.UpdateState();

            item.GetRoom().GetGameMap().UpdateMapForItem(item);
            item.GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerStateChanges, session.GetHabbo(), item);
            //Item.GetRoom().GenerateMaps();
        }

        public void OnWiredTrigger(Item item)
        {
            int modes = item.GetBaseItem().Modes - 1;

            if (modes <= 0)
            {
                item.UpdateState(false, true);
            }

            int currentMode = 0;
            int newMode = 0;

            if (!int.TryParse(item.ExtraData, out currentMode))
            {
            }

            if (currentMode <= 0)
            {
                newMode = 1;
            }
            else if (currentMode >= modes)
            {
                newMode = 0;
            }
            else
            {
                newMode = currentMode + 1;
            }

            if (newMode == 0)
            {
                if (!item.GetRoom().GetGameMap().ItemCanBePlaced(item.GetX, item.GetY))
                {
                    return;
                }
            }

            item.ExtraData = newMode.ToString();
            item.UpdateState();

            item.GetRoom().GetGameMap().UpdateMapForItem(item);
            //Item.GetRoom().GenerateMaps();
        }
    }
}