using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("Enabled", "ChanceWeight", Name = "idx_rp_trash_rewards_enabled_weight")]
[Table("rp_trash_rewards")]
public partial class RpTrashRewardEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("reward_type", TypeName = "tinyint(4)")]
    public int RewardType { get; set; }

    [Column("item_id", TypeName = "int(11)")]
    public int ItemId { get; set; }

    [Column("chance_weight", TypeName = "int(11)")]
    public int ChanceWeight { get; set; }

    [Column("min_amount", TypeName = "int(11)")]
    public int MinAmount { get; set; }

    [Column("max_amount", TypeName = "int(11)")]
    public int MaxAmount { get; set; }

    [Required]
    [Column("enabled")]
    public bool? Enabled { get; set; }
}