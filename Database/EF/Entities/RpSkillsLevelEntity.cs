using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("rp_skills_levels")]
public partial class RpSkillsLevelEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("skill_id", TypeName = "int(11)")]
    public int SkillId { get; set; }

    [Required]
    [StringLength(25)]
    [Column("level")]
    public string Level { get; set; }

    [Column("required_progress", TypeName = "int(11)")]
    public int RequiredProgress { get; set; }
}