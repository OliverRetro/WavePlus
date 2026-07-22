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
            return db.Users
                .Where(u => EF.Functions.Like(u.Username, pattern))
                .Select(u => new { u.Id, u.Username, u.Motto, u.Look, u.LastOnline })
                .Take(50)
                .AsEnumerable()
                .Select(u => new SearchResult(u.Id, u.Username, u.Motto, u.Look, u.LastOnline.HasValue ? u.LastOnline.Value.ToString() : string.Empty))
                .ToList();
        }
    }
}