using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Quests;

namespace Plus.HabboHotel.Items.Interactor
{
    internal class InteractorMultiHeight : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
        }

        public void OnRemove(GameClient session, Item item)
        {
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            if (session == null || !hasRights)
                return;

            PlusEnvironment.GetGame().GetQuestManager().ProgressUserQuest(session, QuestType.FurniSwitch);

            Cycle(item);
        }

        public void OnWiredTrigger(Item item)
        {
            Cycle(item);
        }

        private static void Cycle(Item item)
        {
            int states = item.StateCount;

            if (states <= 1)
                return;

            int next = ((item.HeightState + 1) % states + states) % states;

            item.ExtraData = next.ToString();
            item.UpdateState();
        }
    }
}