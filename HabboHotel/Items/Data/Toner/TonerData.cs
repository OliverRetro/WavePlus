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

            var row = db.RoomItemsToners.Where(t => t.Id == idKey).Select(t => new { t.Enabled, t.Data1, t.Data2, t.Data3 }).FirstOrDefault();

            if (row == null) {
                db.Database.ExecuteSqlInterpolated($"INSERT INTO `room_items_toner` VALUES ({ItemId}, '0', 0, 0, 0)");
                row = db.RoomItemsToners.Where(t => t.Id == idKey).Select(t => new { t.Enabled, t.Data1, t.Data2, t.Data3 }).FirstOrDefault();
            }

            Enabled = int.Parse(row.Enabled);
            Hue = row.Data1;
            Saturation = row.Data2;
            Lightness = row.Data3;
        }
    }
}