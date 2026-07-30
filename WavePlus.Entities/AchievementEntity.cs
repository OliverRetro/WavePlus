using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("achievements")]
public partial class AchievementEntity
{
    [Key]
    [Column("id", TypeName = "int(10) unsigned")]
    public uint Id { get; set; }

    [Required]
    [StringLength(64)]
    [Column("group_name")]
    public string GroupName { get; set; }

    [Required]
    [StringLength(25)]
    [Column("category")]
    public string Category { get; set; }

    [Column("level", TypeName = "int(11)")]
    public int Level { get; set; }

    [Column("reward_amount", TypeName = "int(11)")]
    public int RewardAmount { get; set; }

    // Currency the reward_amount is paid in: -1 = credits, 5 = diamonds, 0 = duckets (default).
    [Column("points_type", TypeName = "int(11)")]
    public int PointsType { get; set; }

    [Column("reward_points", TypeName = "int(11)")]
    public int RewardPoints { get; set; }

    [Column("progress_needed", TypeName = "int(11)")]
    public int ProgressNeeded { get; set; }

    [Column("game_id", TypeName = "int(11)")]
    public int GameId { get; set; }

    //1 = achievement is active and can be loaded/progressed, 0 = disabled
    [Column("enabled")]
    public sbyte Enabled { get; set; }
}