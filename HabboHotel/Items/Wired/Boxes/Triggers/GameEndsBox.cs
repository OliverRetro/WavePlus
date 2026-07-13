using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Items.Wired.Boxes.Triggers
{
    internal class GameEndsBox : IWiredItem
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.TriggerGameEnds;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }
        public string ItemsData { get; set; }

        public GameEndsBox(Room instance, Item item)
        {
            Item = item;
            Instance = instance;
            SetItems = new ConcurrentDictionary<int, Item>();
        }

        public void HandleSave(ClientPacket packet)
        {
        }

        public bool Execute(params object[] @params)
        {
            ICollection<IWiredItem> effects = Instance.GetWired().GetEffects(this);
            ICollection<IWiredItem> conditions = Instance.GetWired().GetConditions(this);

            foreach (IWiredItem condition in conditions)
            {
                Instance.GetWired().OnEvent(condition.Item);
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
                    foreach (RoomUser user in Instance.GetRoomUserManager().GetRoomUsers().ToList())
                    {
                        if (user == null || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                            continue;

                        effect.Execute(user.GetClient().GetHabbo());
                    }

                    Instance.GetWired().OnEvent(effect.Item);
                }
            }

            return true;
        }
    }
}