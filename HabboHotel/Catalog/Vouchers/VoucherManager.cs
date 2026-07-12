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

            using (WavePlusContext db = PlusEnvironment.GetDbContext())
            {
                var rows = db.catalog_vouchers.Where(v => v.enabled == "1")
                    .Select(v => new { v.voucher, v.type, v.value, v.current_uses, v.max_uses }).ToList();
                foreach (var row in rows)
                {
                    _vouchers.Add(row.voucher, new Voucher(row.voucher, row.type, row.value, row.current_uses, row.max_uses));
                }
            }
        }

        public bool TryGetVoucher(string code, out Voucher voucher)
        {
            return _vouchers.TryGetValue(code, out voucher);
        }
    }
}