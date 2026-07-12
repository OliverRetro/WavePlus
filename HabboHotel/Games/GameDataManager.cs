using System.Collections.Generic;
using System.Linq;
using log4net;
using Plus.Database.EF;

namespace Plus.HabboHotel.Games
{
    public class GameDataManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(GameDataManager));

        private readonly Dictionary<int, GameData> _games;

        public GameDataManager()
        {
            _games = new Dictionary<int, GameData>();
        }

        public void Init()
        {
            if (_games.Count > 0)
                _games.Clear();

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var rows = db.games_configs
                    .Select(g => new { g.id, g.name, g.colour_one, g.colour_two, g.resource_path, g.string_three, g.game_swf, g.game_assets, g.game_server_host, g.game_server_port, g.socket_policy_port, g.game_enabled })
                    .ToList();
                foreach (var row in rows)
                {
                    _games.Add(row.id, new GameData(row.id, row.name, row.colour_one, row.colour_two, row.resource_path, row.string_three, row.game_swf, row.game_assets, row.game_server_host, row.game_server_port, row.socket_policy_port, PlusEnvironment.EnumToBool(row.game_enabled)));
                }
            }

            Log.Info("Game Data Manager -> LOADED");
        }

        public bool TryGetGame(int gameId, out GameData data)
        {
            return _games.TryGetValue(gameId, out data);
        }

        public int GetCount()
        {
            return _games.Values.Count(x => x.Enabled);
        }

        public ICollection<GameData> GameData => _games.Values;
    }
}
