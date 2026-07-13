using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorScoreboard : IFurniInteractor
    {
        public void OnPlace(GameClient session, Item item)
        {
        }

        public void OnRemove(GameClient session, Item item)
        {
        }

        public void OnTrigger(GameClient session, Item item, int request, bool hasRights)
        {
            if (!hasRights)
            {
                return;
            }

            // Request 1 - Decrease value with red button
            // Request 2 - Increase value with green button
            // Request 3 - Reset with UI/Wired/Double click

            // Find out what number we are on right now
            if (!int.TryParse(item.ExtraData, out int oldValue))
            {
                oldValue = 0;
            }

            // Decrease value with red button
            if (oldValue >= 0 && oldValue <= 99 && request == 1)
            {
                if (oldValue > 0)
                    oldValue--;
                else if (oldValue == 0)
                    oldValue = 99;
            }

            // Increase value with green button
            if (oldValue >= 0 && oldValue <= 99 && request == 2)
            {
                if (oldValue < 99)
                    oldValue++;
                else if (oldValue == 99)
                    oldValue = 0;
            }

            // Reset with UI/Wired/Double click
            if (request == 3)
            {
                oldValue = 0;
                item.PendingReset = true;
            }

            item.ExtraData = oldValue.ToString();
            item.UpdateState();
        }

        public void OnWiredTrigger(Item item)
        {
            // Always reset scoreboard on Wired trigger
            item.ExtraData = "0";
            item.UpdateState();
        }
    }
}