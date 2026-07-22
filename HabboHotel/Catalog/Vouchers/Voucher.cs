using System.Linq;
using Microsoft.EntityFrameworkCore;
using Plus.Database.EF;

namespace Plus.HabboHotel.Catalog.Vouchers
{
    public class Voucher
    {
        public Voucher(string code, string type, int value, int currentUses, int maxUses)
        {
            Code = code;
            Type = VoucherUtility.GetType(type);
            Value = value;
            CurrentUses = currentUses;
            MaxUses = maxUses;
        }

        public void UpdateUses()
        {
            CurrentUses += 1;
            string code = Code;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                db.CatalogVouchers.Where(v => v.Voucher == code).ExecuteUpdate(s => s.SetProperty(v => v.CurrentUses, v => v.CurrentUses + 1));
            }
        }

        public string Code { get; set; }

        public VoucherType Type { get; set; }

        public int Value { get; set; }

        public int CurrentUses { get; set; }

        public int MaxUses { get; set; }
    }
}