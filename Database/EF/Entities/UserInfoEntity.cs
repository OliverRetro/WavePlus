using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("user_info")]
[Index("UserId", Name = "user_id", IsUnique = true)]
public partial class UserInfoEntity
{
    [Key]
    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Column("bans", TypeName = "int(11)")]
    public int Bans { get; set; }

    [Column("cautions", TypeName = "int(11)")]
    public int Cautions { get; set; }

    [Column("reg_timestamp")]
    public double RegTimestamp { get; set; }

    [Column("login_timestamp")]
    public double LoginTimestamp { get; set; }

    [Column("cfhs", TypeName = "int(11)")]
    public int Cfhs { get; set; }

    [Column("cfhs_abusive", TypeName = "int(11)")]
    public int CfhsAbusive { get; set; }

    [Column("trading_locked")]
    public double TradingLocked { get; set; }

    [Column("trading_locks_count", TypeName = "int(11)")]
    public int TradingLocksCount { get; set; }
}