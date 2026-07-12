using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("user_id", "timestamp", Name = "idx_user_rp_bank_logs_user_time")]
public partial class user_rp_bank_log
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int user_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int amount { get; set; }

    [Required]
    [StringLength(32)]
    public string action_type { get; set; }

    [Required]
    [StringLength(32)]
    public string management_type { get; set; }

    [Column(TypeName = "int(11)")]
    public int fee_paid { get; set; }

    [MaxLength(6)]
    public DateTime timestamp { get; set; }
}
