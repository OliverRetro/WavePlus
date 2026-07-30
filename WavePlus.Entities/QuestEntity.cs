using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("quests")]
public partial class QuestEntity
{
    [Key]
    [Column("id", TypeName = "int(10) unsigned")]
    public uint Id { get; set; }

    [Required]
    [StringLength(32)]
    [Column("type")]
    public string Type { get; set; }

    [Column("level_num", TypeName = "int(11)")]
    public int LevelNum { get; set; }

    [Column("goal_type", TypeName = "int(10)")]
    public int GoalType { get; set; }

    [Column("goal_data", TypeName = "int(10) unsigned")]
    public uint GoalData { get; set; }

    [Required]
    [StringLength(32)]
    [Column("action")]
    public string Action { get; set; }

    [Column("pixel_reward", TypeName = "int(11)")]
    public int PixelReward { get; set; }

    [Required]
    [StringLength(2)]
    [Column("data_bit")]
    public string DataBit { get; set; }

    [Required]
    [Column("reward_type", TypeName = "enum('0','1','2','3','4','5')")]
    public string RewardType { get; set; }

    [Column("timestamp_unlock", TypeName = "int(11)")]
    public int TimestampUnlock { get; set; }

    [Column("timestamp_lock", TypeName = "int(11)")]
    public int TimestampLock { get; set; }
}