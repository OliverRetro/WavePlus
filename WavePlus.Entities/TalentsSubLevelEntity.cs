using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("talents_sub_levels")]
public partial class TalentsSubLevelEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("talent_level", TypeName = "int(11)")]
    public int TalentLevel { get; set; }

    [Column("sub_level", TypeName = "int(11)")]
    public int SubLevel { get; set; }

    [Required]
    [StringLength(45)]
    [Column("badge_code")]
    public string BadgeCode { get; set; }

    [Column("required_progress", TypeName = "int(11)")]
    public int RequiredProgress { get; set; }
}