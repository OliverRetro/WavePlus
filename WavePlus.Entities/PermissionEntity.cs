using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("permissions")]
public partial class PermissionEntity
{
    [Key]
    [Column("id", TypeName = "int(3)")]
    public int Id { get; set; }

    [Required]
    [Column("permission")]
    [StringLength(50)]
    public string Permission1 { get; set; }

    [Required]
    [Column("description", TypeName = "text")]
    public string Description { get; set; }
}