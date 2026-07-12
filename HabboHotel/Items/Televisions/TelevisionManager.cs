using System.Collections.Generic;
using System.Linq;
using log4net;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;

namespace Plus.HabboHotel.Items.Televisions
{
    public class TelevisionManager
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(TelevisionManager));

        public Dictionary<int, TelevisionItem> Televisions;

        public TelevisionManager()
        {
            Televisions = new Dictionary<int, TelevisionItem>();
        }

        public void Init()
        {
            if (Televisions.Count > 0)
                Televisions.Clear();

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                foreach (var row in db.items_youtubes.AsNoTracking().OrderByDescending(x => x.id).ToList())
                {
                    Televisions.Add(row.id, new TelevisionItem(row.id, row.youtube_id, row.title, row.description, PlusEnvironment.EnumToBool(row.enabled)));
                }
            }


            Log.Info("Television Items -> LOADED");
        }


        public ICollection<TelevisionItem> TelevisionList => Televisions.Values;

        public bool TryGet(int itemId, out TelevisionItem televisionItem)
        {
            if (Televisions.TryGetValue(itemId, out televisionItem))
                return true;
            return false;
        }
    }
}