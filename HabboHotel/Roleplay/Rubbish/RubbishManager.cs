using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users;

namespace Plus.HabboHotel.Roleplay.Rubbish
{
    public class RubbishManager
    {
        private const int MinSpawnMs = 60000;
        private const int MaxSpawnMs = 120000;
        private const int MaxPerRoom = 10;
        private const double MaxFloorZ = 0.2;
        private const int ScanIntervalMs = 1000;
        private const int SpawnTileTries = 40;

        private static int _nextRubbishId = 2_000_000_000;

        private readonly ConcurrentDictionary<int, RubbishRoom> _rooms = new();
        private readonly Random _random = new();

        private long _nextScanMs;

        public void OnCycle()
        {
            long now = PlusEnvironment.Now();
            if (now < _nextScanMs)
                return;
            _nextScanMs = now + ScanIntervalMs;

            int baseId = int.TryParse(PlusEnvironment.GetSettingsManager().TryGetValue("rp.rubbish.base.id"), out int id) ? id : 0;
            if (baseId <= 0 || !PlusEnvironment.GetGame().GetItemManager().GetItem(baseId, out ItemData data))
                return;

            foreach (Room room in PlusEnvironment.GetGame().GetRoomManager().GetRooms()) {
                try {
                    if (room == null || room.IsCrashed || room.Group == null || !GroupManager.IsWorkableKind(room.Group.Kind))
                        continue;

                    ProcessRoom(room, baseId, data, now);
                } catch (Exception e) {
                    Core.ExceptionLogger.LogException(e);
                }
            }
        }

        private void ProcessRoom(Room room, int baseId, ItemData data, long now)
        {
            RubbishRoom state = _rooms.GetOrAdd(room.Id, _ => new RubbishRoom { NextSpawnMs = now + _random.Next(MinSpawnMs, MaxSpawnMs + 1) });

            lock (state.Lock) {
                state.Ids.RemoveWhere(itemId => room.GetRoomItemHandler().GetItem(itemId) == null);

                if (now < state.NextSpawnMs || state.Ids.Count >= MaxPerRoom)
                    return;

                state.NextSpawnMs = now + _random.Next(MinSpawnMs, MaxSpawnMs + 1);
                Spawn(room, state, baseId, data);
            }
        }

        private void Spawn(Room room, RubbishRoom state, int baseId, ItemData data)
        {
            Gamemap map = room.GetGameMap();
            if (map?.Model == null)
                return;

            int sizeX = map.Model.MapSizeX;
            int sizeY = map.Model.MapSizeY;

            for (int i = 0; i < SpawnTileTries; i++) {
                int x = _random.Next(0, sizeX);
                int y = _random.Next(0, sizeY);

                if (!map.ValidTile(x, y) || !map.SquareIsOpen(x, y, false) || map.SquareHasUsers(x, y))
                    continue;
                if (map.Model.SqFloorHeight[x, y] > MaxFloorZ)
                    continue;

                int mode = data.Modes > 1 ? _random.Next(1, data.Modes + 1) : 1;
                int itemId = Interlocked.Decrement(ref _nextRubbishId);

                Item item = new(itemId, room.Id, baseId, mode.ToString(), x, y, 0, 0, 0, 0, 0, 0, string.Empty, room);
                if (item.Data == null)
                    return;

                if (room.GetRoomItemHandler().SetFloorItem(null, item, x, y, 0, true, false, true)) {
                    state.Ids.Add(itemId);
                    return;
                }
            }
        }

        public void TryPickup(GameClient session, Item item)
        {
            Habbo habbo = session?.GetHabbo();
            Room room = item?.GetRoom();
            if (habbo == null || room?.Group == null || !GroupManager.IsWorkableKind(room.Group.Kind))
                return;

            RoomUser user = room.GetRoomUserManager().GetRoomUserByHabbo(habbo.Id);
            if (user == null || !Gamemap.TilesTouching(item.GetX, item.GetY, user.X, user.Y))
                return;

            if (!PlusEnvironment.GetGame().GetShiftManager().IsWorkingFor(habbo.Id, room.Group.Id)) {
                session.SendWhisper("You need to be on shift to tidy up.", 1);
                return;
            }

            int tip = int.TryParse(PlusEnvironment.GetSettingsManager().TryGetValue("rp.rubbish.tip"), out int amount) && amount > 0 ? amount : 1;

            if (_rooms.TryGetValue(room.Id, out RubbishRoom state)) {
                lock (state.Lock)
                    state.Ids.Remove(item.Id);
            }

            room.GetRoomItemHandler().RemoveFurniture(null, item.Id);

            habbo.Credits += tip;
            session.SendPacket(new CreditBalanceComposer(habbo.Credits));

            room.SendPacket(new ShoutComposer(user.VirtualId, "*picks up rubbish*", 0, habbo.CustomBubbleId, isRpAction: true));
            session.SendWhisper($"You got tipped ${tip} for tidying the place up.", 1);
        }

        private class RubbishRoom
        {
            public long NextSpawnMs;
            public readonly object Lock = new();
            public readonly HashSet<int> Ids = [];
        }
    }
}