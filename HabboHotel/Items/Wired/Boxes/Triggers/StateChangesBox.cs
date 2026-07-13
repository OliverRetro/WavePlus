using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Items.Wired.Boxes.Triggers
{
    internal class StateChangesBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.TriggerStateChanges;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public StateChangesBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket packet)
        {
            int unknown = packet.PopInt();
            string unknown2 = packet.PopString();

            if (SetItems.Count > 0)
                SetItems.Clear();

            int furniCount = packet.PopInt();
            for (int i = 0; i < furniCount; i++)
            {
                Item selectedItem = Instance.GetRoomItemHandler().GetItem(packet.PopInt());
                if (selectedItem != null)
                    SetItems.TryAdd(selectedItem.Id, selectedItem);
            }
        }

        public bool Execute(params object[] @params)
        {
            Habbo player = (Habbo)@params[0];
            if (player == null)
                return false;

            Item item = (Item)@params[1];
            if (item == null)
                return false;

            if (!SetItems.ContainsKey(item.Id))
                return false;

            ICollection<IWiredItem> effects = Instance.GetWired().GetEffects(this);
            ICollection<IWiredItem> conditions = Instance.GetWired().GetConditions(this);

            foreach (IWiredItem condition in conditions)
            {
                if (!condition.Execute(player))
                    return false;

                Instance?.GetWired().OnEvent(condition.Item);
            }

            //Check the ICollection to find the random addon effect.
            bool hasRandomEffectAddon = effects.Count(x => x.Type == WiredBoxType.AddonRandomEffect) > 0;
            if (hasRandomEffectAddon)
            {
                //Okay, so we have a random addon effect, now lets get the IWiredItem and attempt to execute it.
                IWiredItem randomBox = effects.FirstOrDefault(x => x.Type == WiredBoxType.AddonRandomEffect);
                if (!randomBox.Execute())
                    return false;

                //Success! Let's get our selected box and continue.
                IWiredItem selectedBox = Instance.GetWired().GetRandomEffect(effects.ToList());
                if (!selectedBox.Execute())
                    return false;

                //Woo! Almost there captain, now lets broadcast the update to the room instance.
                if (Instance != null)
                {
                    Instance.GetWired().OnEvent(randomBox.Item);
                    Instance.GetWired().OnEvent(selectedBox.Item);
                }
            }
            else
            {
                foreach (IWiredItem effect in effects)
                {
                    if (!effect.Execute(player))
                        return false;

                    Instance?.GetWired().OnEvent(effect.Item);
                }
            }

            return true;
        }
    }
}