using System;
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

    [Column("created_at", TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at", TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }
}
