using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("permissions_groups")]
public partial class PermissionsGroupEntity
{
    [Key]
    [Column("id", TypeName = "int(3)")]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Column("name")]
    public string Name { get; set; }

    [Required]
    [StringLength(50)]
    [Column("description")]
    public string Description { get; set; }

    [Required]
    [StringLength(12)]
    [Column("badge_code")]
    public string BadgeCode { get; set; }

    [Column("level", TypeName = "int(11)")]
    public int Level { get; set; }
}