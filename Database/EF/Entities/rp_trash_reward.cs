using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("enabled", "chance_weight", Name = "idx_rp_trash_rewards_enabled_weight")]
public partial class rp_trash_reward
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int item_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int chance_weight { get; set; }

    [Column(TypeName = "int(11)")]
    public int min_amount { get; set; }

    [Column(TypeName = "int(11)")]
    public int max_amount { get; set; }

    [Required]
    public bool? enabled { get; set; }
}
