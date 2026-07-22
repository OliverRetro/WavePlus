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

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var rows = db.GamesConfigs
                    .Select(g => new { g.Id, g.Name, g.ColourOne, g.ColourTwo, g.ResourcePath, g.StringThree, g.GameSwf, g.GameAssets, g.GameServerHost, g.GameServerPort, g.SocketPolicyPort, g.GameEnabled })
                    .ToList();
                foreach (var row in rows) {
                    _games.Add(row.Id, new GameData(row.Id, row.Name, row.ColourOne, row.ColourTwo, row.ResourcePath, row.StringThree, row.GameSwf, row.GameAssets, row.GameServerHost, row.GameServerPort, row.SocketPolicyPort, PlusEnvironment.EnumToBool(row.GameEnabled)));
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