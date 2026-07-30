using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("server_rewards")]
public partial class ServerRewardEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("reward_start", TypeName = "int(11)")]
    public int RewardStart { get; set; }

    [Column("reward_end", TypeName = "int(11)")]
    public int RewardEnd { get; set; }

    [Required]
    [Column("reward_type", TypeName = "enum('credits','badge','diamonds','duckets','none')")]
    public string RewardType { get; set; }

    [Required]
    [StringLength(255)]
    [Column("reward_data")]
    public string RewardData { get; set; }

    [Required]
    [StringLength(255)]
    [Column("message")]
    public string Message { get; set; }

    [Required]
    [Column("enabled", TypeName = "enum('1','0')")]
    public string Enabled { get; set; }
}