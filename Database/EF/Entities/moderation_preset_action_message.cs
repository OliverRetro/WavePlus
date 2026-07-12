using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class moderation_preset_action_message
{
    [Key]
    [Column(TypeName = "int(10) unsigned")]
    public uint id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint parent_id { get; set; }

    [Required]
    [StringLength(32)]
    public string caption { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string message_text { get; set; }

    [Column(TypeName = "int(11)")]
    public int mute_hours { get; set; }

    [Column(TypeName = "int(11)")]
    public int ban_hours { get; set; }

    [Column(TypeName = "int(11)")]
    public int ip_ban_hours { get; set; }

    [Column(TypeName = "int(11)")]
    public int trade_lock_days { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string notice { get; set; }
}
