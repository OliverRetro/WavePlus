using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class moderation_topic_action
{
    [Key]
    [Column(TypeName = "int(11) unsigned")]
    public uint id { get; set; }

    [Column(TypeName = "int(11)")]
    public int parent_id { get; set; }

    [Required]
    [StringLength(255)]
    public string type { get; set; }

    [Required]
    [StringLength(225)]
    public string caption { get; set; }

    [Required]
    [StringLength(255)]
    public string message_text { get; set; }

    [Required]
    [StringLength(255)]
    public string default_sanction { get; set; }

    [Column(TypeName = "int(11)")]
    public int mute_time { get; set; }

    [Column(TypeName = "int(11)")]
    public int ban_time { get; set; }

    [Column(TypeName = "int(11)")]
    public int ip_time { get; set; }

    [Column(TypeName = "int(11)")]
    public int trade_lock_time { get; set; }
}
