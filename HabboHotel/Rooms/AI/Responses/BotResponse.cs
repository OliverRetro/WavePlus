using System.Collections.Generic;
using Plus.HabboHotel.Catalog.Utilities;

namespace Plus.HabboHotel.Rooms.AI.Responses
{
    public class BotResponse
    {
        public BotAIType AiType { get; set; }
        public List<string> Keywords { get; set; }
        public string ResponseText { get; set; }
        public string ResponseType { get; set; }
        public List<int> BeverageIds { get; }

        public BotResponse(string botAi, string keywords, string responseText, string responseMode, string responseBeverages)
        {
            AiType = BotUtility.GetAIFromString(botAi);

            Keywords = new List<string>();
            foreach (string keyword in keywords.Split(','))
            {
                Keywords.Add(keyword.ToLower());
            }

            ResponseText = responseText;
            ResponseType = responseMode;

            BeverageIds = new List<int>();
            if (responseBeverages.Contains(","))
            {
                foreach (string vendingId in responseBeverages.Split(','))
                {
                    try
                    {
                        BeverageIds.Add(int.Parse(vendingId));
                    }
                    catch
                    {
                    }
                }
            }
            else if (!string.IsNullOrEmpty(responseBeverages) && (int.Parse(responseBeverages)) > 0)
                BeverageIds.Add(int.Parse(responseBeverages));
        }

        public bool KeywordMatched(string message)
        {
            foreach (string keyword in Keywords)
            {
                if (message.ToLower().Contains(keyword.ToLower()))
                    return true;
            }

            return false;
        }
    }
}