using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class ExecuteWiredStacksBox : IWiredItem
    {
        public Room Instance { get; set; }

        public Item Item { get; set; }

        public WiredBoxType Type => WiredBoxType.EffectExecuteWiredStacks;

        public ConcurrentDictionary<int, Item> SetItems { get; set; }

        public string StringData { get; set; }

        public bool BoolData { get; set; }

        public string ItemsData { get; set; }

        public ExecuteWiredStacksBox(Room instance, Item item)
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
            if (@params.Length != 1)
                return false;

            Habbo player = (Habbo)@params[0];
            if (player == null)
                return false;

            foreach (Item item in SetItems.Values.ToList())
            {
                if (item == null || !Instance.GetRoomItemHandler().GetFloor.Contains(item) || !item.IsWired)
                    continue;

                if (Instance.GetWired().TryGet(item.Id, out IWiredItem wiredItem))
                {
                    if (wiredItem.Type == WiredBoxType.EffectExecuteWiredStacks)
                        continue;
                    ICollection<IWiredItem> effects = Instance.GetWired().GetEffects(wiredItem);
                    if (effects.Count > 0)
                    {
                        foreach (IWiredItem effectItem in effects.ToList())
                        {
                            if (SetItems.ContainsKey(effectItem.Item.Id) && effectItem.Item.Id != item.Id)
                                continue;
                            if (effectItem.Type == WiredBoxType.EffectExecuteWiredStacks)
                                continue;
                            effectItem.Execute(player);
                        }
                    }
                }
                else continue;
            }

            return true;
        }
    }
}