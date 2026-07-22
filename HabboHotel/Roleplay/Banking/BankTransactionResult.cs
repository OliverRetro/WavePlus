namespace Plus.HabboHotel.Roleplay.Banking
{
    public class BankTransactionResult
    {
        public bool Success { get; }
        public int FeePaid { get; }
        public int NetAmount { get; }

        public BankTransactionResult(bool success, int feePaid, int netAmount)
        {
            Success = success;
            FeePaid = feePaid;
            NetAmount = netAmount;
        }
    }
}