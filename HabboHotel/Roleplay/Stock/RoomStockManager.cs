using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;
using Plus.Database.EF.Entities;

namespace Plus.HabboHotel.Roleplay.Stock
{
    public sealed class RoomStockManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RoomStockManager));

        private sealed class StockEntry
        {
            public int RowId;
            public int Amount;
        }

        private readonly Dictionary<(int RoomId, string StockType, int ItemId), StockEntry> _stock = new();
        private readonly HashSet<(int RoomId, string StockType, int ItemId)> _dirty = [];
        private readonly object _sync = new();

        public void Init()
        {
            lock (_sync)
            {
                _stock.Clear();
                _dirty.Clear();

                using WavePlusContext db = PlusEnvironment.GetDbContext();

                foreach (RoomItemStockEntity row in db.RoomItemStocks.AsNoTracking().ToList())
                    _stock[(row.RoomId, row.StockType, row.ItemId)] = new StockEntry { RowId = row.Id, Amount = row.Amount };

                Log.Info($"[RP] Loaded {_stock.Count} room stock entries.");
            }
        }

        public int GetStock(int roomId, string stockType, int itemId)
        {
            lock (_sync)
                return _stock.TryGetValue((roomId, stockType, itemId), out StockEntry entry) ? entry.Amount : 0;
        }

        /// <summary>Every stocked id in a room, including entries that have run down to zero.</summary>
        public List<(string StockType, int ItemId, int Amount)> GetRoomStock(int roomId)
        {
            lock (_sync)
            {
                return _stock.Where(kv => kv.Key.RoomId == roomId)
                    .Select(kv => (kv.Key.StockType, kv.Key.ItemId, kv.Value.Amount))
                    .OrderBy(s => s.StockType).ThenBy(s => s.ItemId)
                    .ToList();
            }
        }

        /// <summary>Adds (or removes, with a negative amount) stock. Returns the resulting amount, never below zero.</summary>
        public int AddStock(int roomId, string stockType, int itemId, int amount)
        {
            lock (_sync)
            {
                var key = (roomId, stockType, itemId);

                if (!_stock.TryGetValue(key, out StockEntry entry))
                {
                    entry = new StockEntry();
                    _stock[key] = entry;
                }

                entry.Amount = Math.Max(0, entry.Amount + amount);
                _dirty.Add(key);
                return entry.Amount;
            }
        }

        public void SetStock(int roomId, string stockType, int itemId, int amount)
        {
            lock (_sync)
            {
                var key = (roomId, stockType, itemId);

                if (!_stock.TryGetValue(key, out StockEntry entry))
                {
                    entry = new StockEntry();
                    _stock[key] = entry;
                }

                entry.Amount = Math.Max(0, amount);
                _dirty.Add(key);
            }
        }

        /// <summary>
        /// Takes <paramref name="amount"/> off the room's stock, all or nothing. This is the call a
        /// sale goes through: false means the room has run out and the sale must not happen.
        /// </summary>
        public bool TryConsume(int roomId, string stockType, int itemId, int amount = 1)
        {
            if (amount <= 0)
                return false;

            lock (_sync)
            {
                var key = (roomId, stockType, itemId);

                if (!_stock.TryGetValue(key, out StockEntry entry) || entry.Amount < amount)
                    return false;

                entry.Amount -= amount;
                _dirty.Add(key);
                return true;
            }
        }

        /// <summary>Writes every dirty entry to the database. Registered as a world saver.</summary>
        public void Save()
        {
            List<((int RoomId, string StockType, int ItemId) Key, StockEntry Entry)> pending;

            lock (_sync)
            {
                if (_dirty.Count == 0)
                    return;

                pending = _dirty.Select(key => (key, _stock[key])).ToList();
                _dirty.Clear();
            }

            try
            {
                using WavePlusContext db = PlusEnvironment.GetDbContext();

                foreach (((int roomId, string stockType, int itemId) key, StockEntry entry) in pending)
                {
                    if (entry.RowId > 0)
                    {
                        int rowId = entry.RowId, amount = entry.Amount;
                        db.RoomItemStocks.Where(s => s.Id == rowId)
                            .ExecuteUpdate(s => s.SetProperty(r => r.Amount, amount));
                        continue;
                    }

                    RoomItemStockEntity row = new()
                    {
                        RoomId = key.roomId,
                        StockType = key.stockType,
                        ItemId = key.itemId,
                        Amount = entry.Amount
                    };

                    db.RoomItemStocks.Add(row);
                    db.SaveChanges();
                    entry.RowId = row.Id;
                }
            }
            catch (Exception ex)
            {
                // Put the entries back so the next flush retries them rather than losing the stock.
                lock (_sync)
                {
                    foreach ((var key, StockEntry _) in pending)
                        _dirty.Add(key);
                }

                Log.Error($"[RP] Failed to save room stock: {ex.Message}");
            }
        }
    }
}
