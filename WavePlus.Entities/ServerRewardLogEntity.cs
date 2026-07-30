using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("server_reward_logs")]
public partial class ServerRewardLogEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Column("reward_id", TypeName = "int(11)")]
    public int RewardId { get; set; }
}