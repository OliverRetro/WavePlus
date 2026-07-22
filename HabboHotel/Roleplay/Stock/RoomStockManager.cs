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

        public const int RoomScope = 0;

        private sealed class StockEntry
        {
            public int RowId;
            public int Amount;
            public int Price;
        }

        private readonly Dictionary<(int RoomId, int FurniId, string StockType, int ItemId), StockEntry> _stock = new();
        private readonly HashSet<(int RoomId, int FurniId, string StockType, int ItemId)> _dirty = [];
        private readonly object _sync = new();

        public void Init()
        {
            lock (_sync) {
                _stock.Clear();
                _dirty.Clear();

                using WavePlusContext db = PlusEnvironment.GetDbContext();

                foreach (RoomItemStockEntity row in db.RoomItemStocks.AsNoTracking().ToList()) {
                    _stock[(row.RoomId, row.FurniId, row.StockType, row.ItemId)] =
                        new StockEntry { RowId = row.Id, Amount = row.Amount, Price = row.Price };
                }

                Log.Info($"[RP] Loaded {_stock.Count} room stock entries.");
            }
        }

        public int GetStock(int roomId, string stockType, int itemId, int furniId = RoomScope)
        {
            lock (_sync)
                return _stock.TryGetValue((roomId, furniId, stockType, itemId), out StockEntry entry) ? entry.Amount : 0;
        }

        public int GetPrice(int roomId, string stockType, int itemId, int furniId = RoomScope)
        {
            lock (_sync)
                return _stock.TryGetValue((roomId, furniId, stockType, itemId), out StockEntry entry) ? entry.Price : 0;
        }

        public List<(string StockType, int ItemId, int Amount, int Price)> GetRoomStock(int roomId, int furniId = RoomScope)
        {
            lock (_sync) {
                return [.. _stock.Where(kv => kv.Key.RoomId == roomId && kv.Key.FurniId == furniId)
                    .Select(kv => (kv.Key.StockType, kv.Key.ItemId, kv.Value.Amount, kv.Value.Price))
                    .OrderBy(s => s.StockType).ThenBy(s => s.ItemId)];
            }
        }

        public List<int> GetStockedFurniIds(int roomId)
        {
            lock (_sync) {
                return _stock.Keys.Where(k => k.RoomId == roomId && k.FurniId != RoomScope)
                    .Select(k => k.FurniId).Distinct().OrderBy(x => x).ToList();
            }
        }

        public int AddStock(int roomId, string stockType, int itemId, int amount, int furniId = RoomScope, int? price = null)
        {
            lock (_sync) {
                StockEntry entry = GetOrCreate(roomId, furniId, stockType, itemId);

                entry.Amount = Math.Max(0, entry.Amount + amount);
                if (price.HasValue)
                    entry.Price = Math.Max(0, price.Value);

                _dirty.Add((roomId, furniId, stockType, itemId));
                return entry.Amount;
            }
        }

        public void SetStock(int roomId, string stockType, int itemId, int amount, int furniId = RoomScope, int? price = null)
        {
            lock (_sync) {
                StockEntry entry = GetOrCreate(roomId, furniId, stockType, itemId);

                entry.Amount = Math.Max(0, amount);
                if (price.HasValue)
                    entry.Price = Math.Max(0, price.Value);

                _dirty.Add((roomId, furniId, stockType, itemId));
            }
        }
        public void ClearFurniStock(int roomId, int furniId)
        {
            if (furniId == RoomScope)
                return;

            lock (_sync) {
                foreach (var key in _stock.Keys.Where(k => k.RoomId == roomId && k.FurniId == furniId).ToList()) {
                    _stock[key].Amount = 0;
                    _dirty.Add(key);
                }
            }
        }

        public bool TryConsume(int roomId, string stockType, int itemId, int amount = 1, int furniId = RoomScope)
        {
            if (amount <= 0)
                return false;

            lock (_sync) {
                var key = (roomId, furniId, stockType, itemId);

                if (!_stock.TryGetValue(key, out StockEntry entry) || entry.Amount < amount)
                    return false;

                entry.Amount -= amount;
                _dirty.Add(key);
                return true;
            }
        }

        private StockEntry GetOrCreate(int roomId, int furniId, string stockType, int itemId)
        {
            var key = (roomId, furniId, stockType, itemId);

            if (!_stock.TryGetValue(key, out StockEntry entry)) {
                entry = new StockEntry();
                _stock[key] = entry;
            }

            return entry;
        }

        public void Save()
        {
            List<((int RoomId, int FurniId, string StockType, int ItemId) Key, StockEntry Entry)> pending;

            lock (_sync) {
                if (_dirty.Count == 0)
                    return;

                pending = _dirty.Select(key => (key, _stock[key])).ToList();
                _dirty.Clear();
            }

            try {
                using WavePlusContext db = PlusEnvironment.GetDbContext();

                foreach (((int roomId, int furniId, string stockType, int itemId) key, StockEntry entry) in pending) {
                    if (entry.RowId > 0) {
                        int rowId = entry.RowId, amount = entry.Amount, price = entry.Price;
                        db.RoomItemStocks.Where(s => s.Id == rowId)
                            .ExecuteUpdate(s => s
                                .SetProperty(r => r.Amount, amount)
                                .SetProperty(r => r.Price, price));
                        continue;
                    }

                    RoomItemStockEntity row = new()
                    {
                        RoomId = key.roomId,
                        FurniId = key.furniId,
                        StockType = key.stockType,
                        ItemId = key.itemId,
                        Amount = entry.Amount,
                        Price = entry.Price
                    };

                    db.RoomItemStocks.Add(row);
                    db.SaveChanges();
                    entry.RowId = row.Id;
                }
            } catch (Exception ex) {
                // Put the entries back so the next flush retries them rather than losing the stock.
                lock (_sync) {
                    foreach ((var key, StockEntry _) in pending)
                        _dirty.Add(key);
                }

                Log.Error($"[RP] Failed to save room stock: {ex.Message}");
            }
        }
    }
}