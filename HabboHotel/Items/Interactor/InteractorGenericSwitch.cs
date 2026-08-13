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
            int modes = item.StateCount - 1;

            if (session == null || !hasRights || modes <= 0) {
                return;
            }

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.FurniSwitch);

            item.ExtraData = NextMode(item, modes).ToString();
            item.UpdateState();
        }

        public void OnWiredTrigger(Item item)
        {
            int modes = item.StateCount - 1;

            if (modes == 0) {
                return;
            }

            if (string.IsNullOrEmpty(item.ExtraData))
                item.ExtraData = "0";

            if (!int.TryParse(item.ExtraData, out int _)) {
                return;
            }

            item.ExtraData = NextMode(item, modes).ToString();
            item.UpdateState();
        }

        private static int NextMode(Item item, int modes)
        {
            int currentMode = item.HeightState;

            if (currentMode <= 0)
                return 1;

            if (currentMode >= modes)
                return 0;

            return currentMode + 1;
        }
    }
}