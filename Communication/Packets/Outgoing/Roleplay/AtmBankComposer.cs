using System;
using System.Globalization;

namespace Plus.Communication.Packets.Outgoing.Roleplay
{
    public class AtmBankComposer : MessageComposer
    {
        private readonly bool _hasAccount;
        private readonly string _accountNumber;
        private readonly int _bankBalance;
        private readonly int _walletBalance;
        private readonly double _feePercentage;
        private readonly bool _canDeposit;
        private readonly bool _canWithdraw;
        private readonly long _updatedAt;

        public AtmBankComposer(bool hasAccount, string accountNumber, int bankBalance, int walletBalance, double feePercentage, bool canDeposit, bool canWithdraw)
            : base(ServerPacketHeader.AtmBankMessageComposer)
        {
            _hasAccount = hasAccount;
            _accountNumber = accountNumber ?? string.Empty;
            _bankBalance = bankBalance;
            _walletBalance = walletBalance;
            _feePercentage = feePercentage;
            _canDeposit = canDeposit;
            _canWithdraw = canWithdraw;
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
        }
    }
}