using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;

namespace Plus.HabboHotel.Items.Data.Toner
{
    public class TonerData
    {
        public int ItemId;
        public int Hue;
        public int Saturation;
        public int Lightness;
        public int Enabled;

        public TonerData(int item)
        {
            ItemId = item;

            uint idKey = (uint)ItemId;
            using WavePlusContext db = PlusEnvironment.GetDbContext();

            var row = db.room_items_toners.Where(t => t.id == idKey).Select(t => new { t.enabled, t.data1, t.data2, t.data3 }).FirstOrDefault();

            if (row == null) {
                db.Database.ExecuteSqlInterpolated($"INSERT INTO `room_items_toner` VALUES ({ItemId}, '0', 0, 0, 0)");
                row = db.room_items_toners.Where(t => t.id == idKey).Select(t => new { t.enabled, t.data1, t.data2, t.data3 }).FirstOrDefault();
            }

            Enabled = int.Parse(row.enabled);
            Hue = row.data1;
            Saturation = row.data2;
            Lightness = row.data3;
        }
    }
}