namespace Plus.HabboHotel.Roleplay.Corporation
{
    public class ActiveShift
    {
        public int UserId { get; }
        public int GroupId { get; }
        public int ShiftPay { get; }
        public int ShiftDurationMinutes { get; }
        public int TicksRemaining { get; set; }
        public int BonusCredits { get; private set; }
        public string CostumeFigure { get; set; }

        public ActiveShift(int userId, int groupId, int shiftPay, int shiftDurationMinutes)
        {
            UserId = userId;
            GroupId = groupId;
            ShiftPay = shiftPay;
            ShiftDurationMinutes = shiftDurationMinutes;
            TicksRemaining = shiftDurationMinutes;
        }

        public void AddBonus(int amount) => BonusCredits += amount;
    }
}