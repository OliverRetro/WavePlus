using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("achievements_talents")]
public partial class AchievementsTalentEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [Column("type", TypeName = "enum('citizenship','status')")]
    public string Type { get; set; }

    [Column("parent_category", TypeName = "int(11)")]
    public int ParentCategory { get; set; }

    [Column("level", TypeName = "int(11)")]
    public int Level { get; set; }

    [Column("order_num", TypeName = "int(11)")]
    public int OrderNum { get; set; }

    [Required]
    [StringLength(255)]
    [Column("achievement_group")]
    public string AchievementGroup { get; set; }

    [Column("achievement_level", TypeName = "int(11)")]
    public int AchievementLevel { get; set; }

    [Required]
    [StringLength(255)]
    [Column("prize")]
    public string Prize { get; set; }

    [Column("prize_baseitem", TypeName = "int(11) unsigned")]
    public uint PrizeBaseitem { get; set; }
}