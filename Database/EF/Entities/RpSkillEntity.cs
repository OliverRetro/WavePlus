using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("rp_skills")]
public partial class RpSkillEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [StringLength(25)]
    [Column("name")]
    public string Name { get; set; }

    [Required]
    [StringLength(25)]
    [Column("progress_category")]
    public string ProgressCategory { get; set; }

    [Required]
    [StringLength(255)]
    [Column("description")]
    public string Description { get; set; }

    [StringLength(255)]
    [Column("badge_code")]
    public string BadgeCode { get; set; }
}