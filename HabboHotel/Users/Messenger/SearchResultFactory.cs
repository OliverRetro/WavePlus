using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;

namespace Plus.HabboHotel.Users.Messenger
{
    public static class SearchResultFactory
    {
        public static List<SearchResult> GetSearchResult(string query)
        {
            string pattern = query + "%";

            using WavePlusContext db = PlusEnvironment.GetDbContext();
            return db.users
                .Where(u => EF.Functions.Like(u.username, pattern))
                .Select(u => new { u.id, u.username, u.motto, u.look, u.last_online })
                .Take(50)
                .AsEnumerable()
                .Select(u => new SearchResult(u.id, u.username, u.motto, u.look, u.last_online.HasValue ? u.last_online.Value.ToString() : string.Empty))
                .ToList();
        }
    }
}
