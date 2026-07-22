using System.Linq;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Database.EF;
using Plus.Database.EF.Entities;
using Plus.HabboHotel.Catalog.Vouchers;
using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Incoming.Catalog
{
    public class RedeemVoucherEvent : IPacketEvent
    {
        public void Parse(GameClient session, ClientPacket packet)
        {
            string code = packet.PopString().Replace("\r", "");

            if (!PlusEnvironment.GetGame().GetCatalog().GetVoucherManager().TryGetVoucher(code, out Voucher voucher)) {
                session.SendPacket(new VoucherRedeemErrorComposer(0));
                return;
            }

            if (voucher.CurrentUses >= voucher.MaxUses) {
                session.SendNotification("Oops, this voucher has reached the maximum usage limit!");
                return;
            }

            int voucherUserId = session.GetHabbo().Id;
            using (WavePlusContext db = PlusEnvironment.GetDbContext()) {
                if (db.UserVouchers.Any(v => v.UserId == voucherUserId && v.Voucher == code)) {
                    session.SendNotification("You've already used this voucher code, one per each user, sorry!");
                    return;
                }

                db.UserVouchers.Add(new UserVoucherEntity
                {
                    UserId = voucherUserId,
                    Voucher = code
                });
                db.SaveChanges();
            }

            voucher.UpdateUses();

            if (voucher.Type == VoucherType.Credit) {
                session.GetHabbo().Credits += voucher.Value;
                session.SendPacket(new CreditBalanceComposer(session.GetHabbo().Credits));
            } else if (voucher.Type == VoucherType.Ducket) {
                session.GetHabbo().Duckets += voucher.Value;
                session.SendPacket(new HabboActivityPointNotificationComposer(session.GetHabbo().Duckets, voucher.Value));
            }

            session.SendPacket(new VoucherRedeemOkComposer());
        }
    }
}