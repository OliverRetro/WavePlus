using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;
using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms.AI.Responses;

namespace Plus.HabboHotel.Bots
{
    public class BotManager
    {
        private readonly List<BotResponse> _responses;

        public BotManager()
        {
            _responses = [];
        }

        public void Init()
        {
            if (_responses.Count > 0)
                _responses.Clear();

            using WavePlusContext db = PlusEnvironment.GetDbContext();

            foreach (var row in db.BotsResponses.Select(r => new { r.BotAi, r.ChatKeywords, r.ResponseText, r.ResponseMode, r.ResponseBeverage }).ToList())
                _responses.Add(new BotResponse(row.BotAi, row.ChatKeywords, row.ResponseText, row.ResponseMode, row.ResponseBeverage));
        }

        public BotResponse GetResponse(BotAIType type, string message)
        {
            foreach (BotResponse response in _responses.Where(x => x.AiType == type).ToList()) {
                if (response.KeywordMatched(message)) {
                    return response;
                }
            }

            return null;
        }
    }
}