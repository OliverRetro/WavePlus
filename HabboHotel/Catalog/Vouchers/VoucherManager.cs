using System.Collections.Generic;
using System.Linq;
using Plus.Database.EF;

namespace Plus.HabboHotel.Catalog.Vouchers
{
    public class VoucherManager
    {
        private readonly Dictionary<string, Voucher> _vouchers;

        public VoucherManager()
        {
            _vouchers = new Dictionary<string, Voucher>();
        }

        public void Init()
        {
            if (_vouchers.Count > 0)
                _vouchers.Clear();

            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                var rows = db.CatalogVouchers.Where(v => v.Enabled == "1")
                    .Select(v => new { v.Voucher, v.Type, v.Value, v.CurrentUses, v.MaxUses }).ToList();
                foreach (var row in rows) {
                    _vouchers.Add(row.Voucher, new Voucher(row.Voucher, row.Type, row.Value, row.CurrentUses, row.MaxUses));
                }
            }
        }

        public bool TryGetVoucher(string code, out Voucher voucher)
        {
            return _vouchers.TryGetValue(code, out voucher);
        }
    }
}