using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Incoming;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Items.Wired.Boxes.Effects
{
    internal class TeleportUserBox : IWiredItem, IWiredCycle
    {
        public Room Instance { get; set; }
        public Item Item { get; set; }
        public WiredBoxType Type => WiredBoxType.EffectTeleportToFurni;
        public ConcurrentDictionary<int, Item> SetItems { get; set; }
        public string StringData { get; set; }
        public bool BoolData { get; set; }

        public int Delay
        {
            get => _delay;
            set
            {
                _delay = value;
                TickCount = value + 1;
            }
        }

        public int TickCount { get; set; }
        public string ItemsData { get; set; }

        private readonly Queue _queue;
        private int _delay;

        public TeleportUserBox(Room instance, Item item)
        {
            Instance = instance;
            Item = item;
            SetItems = new ConcurrentDictionary<int, Item>();

            _queue = new Queue();
            TickCount = Delay;
        }

        public void HandleSave(ClientPacket packet)
        {
            int unknown = packet.PopInt();
            string unknown2 = packet.PopString();

            if (SetItems.Count > 0)
                SetItems.Clear();

            int furniCount = packet.PopInt();
            for (int i = 0; i < furniCount; i++) {
                Item selectedItem = Instance.GetRoomItemHandler().GetItem(packet.PopInt());
                if (selectedItem != null)
                    SetItems.TryAdd(selectedItem.Id, selectedItem);
            }

            Delay = packet.PopInt();
        }

        public bool OnCycle()
        {
            if (_queue.Count == 0 || SetItems.Count == 0) {
                _queue.Clear();
                TickCount = Delay;
                return true;
            }

            while (_queue.Count > 0) {
                Habbo player = (Habbo)_queue.Dequeue();
                if (player == null || player.CurrentRoom != Instance)
                    continue;

                TeleportUser(player);
            }

            TickCount = Delay;
            return true;
        }

        public bool Execute(params object[] @params)
        {
            if (@params == null || @params.Length == 0)
                return false;

            Habbo player = (Habbo)@params[0];

            if (player == null)
                return false;

            if (player.Effects() != null)
                player.Effects().ApplyEffect(4);

            _queue.Enqueue(player);
            return true;
        }

        private void TeleportUser(Habbo player)
        {
            Room room = player?.CurrentRoom;
            if (room == null)
                return;

            RoomUser user = player.CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(player.Username);
            if (user == null)
                return;

            if (player.IsTeleporting || player.IsHopping || player.TeleportId != 0)
                return;

            Random rand = new();
            List<Item> items = SetItems.Values.ToList();
            items = items.OrderBy(x => rand.Next()).ToList();

            if (items.Count == 0)
                return;

            Item item = items.First();
            if (item == null)
                return;

            if (!Instance.GetRoomItemHandler().GetFloor.Contains(item)) {
                SetItems.TryRemove(item.Id, out item);

                if (items.Contains(item))
                    items.Remove(item);

                if (SetItems.Count == 0 || items.Count == 0)
                    return;

                item = items.First();
                if (item == null)
                    return;
            }

            if (room.GetGameMap() == null)
                return;

            room.GetGameMap().TeleportToItem(user, item);
            room.GetRoomUserManager().UpdateUserStatusses();

            if (player.Effects() != null)
                player.Effects().ApplyEffect(0);
        }
    }
}