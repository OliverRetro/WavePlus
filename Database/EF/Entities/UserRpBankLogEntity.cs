using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("UserId", "Timestamp", Name = "idx_user_rp_bank_logs_user_time")]
[Table("user_rp_bank_logs")]
public partial class UserRpBankLogEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Column("amount", TypeName = "int(11)")]
    public int Amount { get; set; }

    [Required]
    [StringLength(32)]
    [Column("action_type")]
    public string ActionType { get; set; }

    [Required]
    [StringLength(32)]
    [Column("management_type")]
    public string ManagementType { get; set; }

    [Column("fee_paid", TypeName = "int(11)")]
    public int FeePaid { get; set; }

    [MaxLength(6)]
    [Column("timestamp")]
    public DateTime Timestamp { get; set; }
}
