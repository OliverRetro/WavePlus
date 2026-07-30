using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("group_permissions")]
public partial class GroupPermissionEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("group_id", TypeName = "int(11)")]
    public int GroupId { get; set; }

    [Column("level_id", TypeName = "int(11)")]
    public int LevelId { get; set; }

    [Column("permission_id", TypeName = "int(11)")]
    public int PermissionId { get; set; }

    [Column("created_at", TypeName = "int(11)")]
    public int CreatedAt { get; set; }

    [Column("updated_at", TypeName = "int(11)")]
    public int? UpdatedAt { get; set; }
}