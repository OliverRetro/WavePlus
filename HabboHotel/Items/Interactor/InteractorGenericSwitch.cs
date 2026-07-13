using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorGenericSwitch : IFurniInteractor
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

            if (session == null || !hasRights || modes <= 0)
            {
                return;
            }

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.FurniSwitch);

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

            item.ExtraData = newMode.ToString();
            item.UpdateState();
        }

        public void OnWiredTrigger(Item item)
        {
            int modes = item.GetBaseItem().Modes - 1;

            if (modes == 0)
            {
                return;
            }

            int currentMode = 0;
            int newMode = 0;

            if (string.IsNullOrEmpty(item.ExtraData))
                item.ExtraData = "0";

            if (!int.TryParse(item.ExtraData, out currentMode))
            {
                return;
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

            item.ExtraData = newMode.ToString();
            item.UpdateState();
        }
    }
}