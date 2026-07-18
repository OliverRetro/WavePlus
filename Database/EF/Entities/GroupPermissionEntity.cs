using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("group_permissions")]
public partial class GroupPermissionEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    /// <summary>
    /// groups.id
    /// </summary>
    [Column("group_id", TypeName = "int(11)")]
    public int GroupId { get; set; }

    /// <summary>
    /// groups_roles.level
    /// </summary>
    [Column("level_id", TypeName = "int(11)")]
    public int LevelId { get; set; }

    /// <summary>
    /// group_permissiosn_keys.id
    /// </summary>
    [Column("permission_id", TypeName = "int(11)")]
    public int PermissionId { get; set; }

    /// <summary>Unix timestamp (seconds) the assignment was created.</summary>
    [Column("created_at", TypeName = "int(11)")]
    public int CreatedAt { get; set; }

    /// <summary>Unix timestamp (seconds) the assignment was last changed, if ever.</summary>
    [Column("updated_at", TypeName = "int(11)")]
    public int? UpdatedAt { get; set; }
}
