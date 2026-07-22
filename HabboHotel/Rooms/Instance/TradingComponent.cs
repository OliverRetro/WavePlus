using System.Collections.Concurrent;
using Plus.HabboHotel.Rooms.Trading;

namespace Plus.HabboHotel.Rooms.Instance
{
    public class TradingComponent
    {
        private int _currentId;
        private readonly Room _instance;
        private readonly ConcurrentDictionary<int, Trade> _activeTrades;

        public TradingComponent(Room instance)
        {
            _currentId = 1;
            _instance = instance;
            _activeTrades = new ConcurrentDictionary<int, Trade>();
        }

        public bool StartTrade(RoomUser player1, RoomUser player2, out Trade trade)
        {
            _currentId++;
            trade = new Trade(_currentId, player1, player2, _instance);
            return _activeTrades.TryAdd(_currentId, trade);
        }

        public bool TryGetTrade(int tradeId, out Trade trade)
        {
            return _activeTrades.TryGetValue(tradeId, out trade);
        }

        public bool RemoveTrade(int id)
        {
            return _activeTrades.TryRemove(id, out Trade trade);
        }

        public void Cleanup()
        {
            foreach (Trade trade in _activeTrades.Values) {
                foreach (TradeUser user in trade.Users) {
                    if (user == null || user.RoomUser == null)
                        continue;

                    trade.EndTrade(user.RoomUser.HabboId);
                }
            }
        }
    }
}