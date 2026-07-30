using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("group_permission_keys")]
public partial class GroupPermissionKeyEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [StringLength(55)]
    [Column("name")]
    public string Name { get; set; }

    [Required]
    [StringLength(55)]
    [Column("key")]
    public string Key { get; set; }

    [Required]
    [StringLength(255)]
    [Column("description")]
    public string Description { get; set; }

    [Required]
    [Column("type_specific", TypeName = "text")]
    public string TypeSpecific { get; set; }
}