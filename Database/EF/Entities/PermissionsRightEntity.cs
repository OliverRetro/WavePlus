using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("permissions_rights")]
public partial class PermissionsRightEntity
{
    [Key]
    [Column("id", TypeName = "int(10)")]
    public int Id { get; set; }

    [Column("group_id", TypeName = "int(10)")]
    public int GroupId { get; set; }

    [Column("permission_id", TypeName = "int(10)")]
    public int PermissionId { get; set; }
}