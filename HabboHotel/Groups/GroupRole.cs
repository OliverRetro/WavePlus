namespace Plus.HabboHotel.Groups
{
    public class GroupRole
    {
        public int Id { get; private set; }
        public int GroupId { get; }
        public int Level { get; }
        public string Name { get; private set; }
        public int ShiftPay { get; private set; }
        public string ShiftCostume { get; private set; }
        public int ShiftDuration { get; private set; }
        public string ShiftMotto { get; private set; }
        public int CreatedAt { get; }

        public bool IsNew { get; private set; }
        public bool Dirty { get; private set; }

        public GroupRole(int id, int groupId, int level, string name, int shiftPay, string shiftCostume, int shiftDuration, string shiftMotto, int createdAt, bool isNew = false)
        {
            Id = id;
            GroupId = groupId;
            Level = level;
            Name = name;
            ShiftPay = shiftPay;
            ShiftCostume = shiftCostume;
            ShiftDuration = shiftDuration;
            ShiftMotto = shiftMotto;
            CreatedAt = createdAt;
            IsNew = isNew;
            Dirty = isNew;
        }

        public void Update(string name, int shiftPay, string shiftCostume, int shiftDuration, string shiftMotto)
        {
            Name = name;
            ShiftPay = shiftPay;
            ShiftCostume = shiftCostume;
            ShiftDuration = shiftDuration;
            ShiftMotto = shiftMotto;
            Dirty = true;
        }

        public void MarkPersisted(int id)
        {
            Id = id;
            IsNew = false;
            Dirty = false;
        }

        public void MarkSaved()
        {
            Dirty = false;
            IsNew = false;
        }
    }
}