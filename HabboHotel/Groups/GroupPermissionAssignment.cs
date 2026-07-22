namespace Plus.HabboHotel.Groups
{
    public class GroupPermissionAssignment
    {
        public int Id { get; private set; }
        public int GroupId { get; }
        public int LevelId { get; }
        public int PermissionId { get; }
        public int CreatedAt { get; }

        public bool IsNew { get; private set; }

        public GroupPermissionAssignment(int id, int groupId, int levelId, int permissionId, int createdAt, bool isNew = false)
        {
            Id = id;
            GroupId = groupId;
            LevelId = levelId;
            PermissionId = permissionId;
            CreatedAt = createdAt;
            IsNew = isNew;
        }

        public void MarkPersisted(int id)
        {
            Id = id;
            IsNew = false;
        }
    }
}