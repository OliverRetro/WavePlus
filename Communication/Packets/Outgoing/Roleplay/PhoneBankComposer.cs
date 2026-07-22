using System;
using System.Collections.Generic;
using System.Globalization;
using Plus.HabboHotel.Roleplay.Banking;

namespace Plus.Communication.Packets.Outgoing.Roleplay
{
    public class PhoneBankComposer : MessageComposer
    {
        private readonly bool _hasAccount;
        private readonly string _accountNumber;
        private readonly int _bankBalance;
        private readonly int _walletBalance;
        private readonly double _feePercentage;
        private readonly bool _canDeposit;
        private readonly bool _canWithdraw;
        private readonly long _updatedAt;
        private readonly List<UserRpBankLog> _transactions;

        public PhoneBankComposer(bool hasAccount, string accountNumber, int bankBalance, int walletBalance, double feePercentage, bool canDeposit, bool canWithdraw, List<UserRpBankLog> transactions)
            : base(ServerPacketHeader.PhoneBankMessageComposer)
        {
            _hasAccount = hasAccount;
            _accountNumber = accountNumber ?? string.Empty;
            _bankBalance = bankBalance;
            _walletBalance = walletBalance;
            _feePercentage = feePercentage;
            _canDeposit = canDeposit;
            _canWithdraw = canWithdraw;
            _transactions = transactions ?? [];
            _updatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteBoolean(_hasAccount);
            packet.WriteString(_accountNumber);
            packet.WriteString(_bankBalance.ToString(CultureInfo.InvariantCulture));
            packet.WriteString(_walletBalance.ToString(CultureInfo.InvariantCulture));
            packet.WriteString((_bankBalance + _walletBalance).ToString(CultureInfo.InvariantCulture));
            packet.WriteString(_feePercentage.ToString("0.##", CultureInfo.InvariantCulture));
            packet.WriteBoolean(_canDeposit);
            packet.WriteBoolean(_canWithdraw);
            packet.WriteString(_updatedAt.ToString(CultureInfo.InvariantCulture));

            packet.WriteInteger(_transactions.Count);
            foreach (UserRpBankLog log in _transactions) {
                packet.WriteInteger(0); // id (not tracked individually)
                packet.WriteString(log.ActionType ?? ""); // DEPOSIT|WITHDRAWAL|...
                packet.WriteString(log.Amount.ToString(CultureInfo.InvariantCulture));
                packet.WriteString(log.FeePaid.ToString(CultureInfo.InvariantCulture));
                packet.WriteInteger(-1); // from_user_id (not tracked)
                packet.WriteInteger(-1); // to_user_id (not tracked)
                packet.WriteInteger(-1); // room_id (not tracked)
                packet.WriteString(log.ManagementType ?? ""); // description
                packet.WriteString((log.Timestamp * 1000L).ToString(CultureInfo.InvariantCulture));
            }
        }
    }
}