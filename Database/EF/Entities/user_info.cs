using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("user_info")]
[Index("user_id", Name = "user_id", IsUnique = true)]
public partial class user_info
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int user_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int bans { get; set; }

    [Column(TypeName = "int(11)")]
    public int cautions { get; set; }

    public double reg_timestamp { get; set; }

    public double login_timestamp { get; set; }

    [Column(TypeName = "int(11)")]
    public int cfhs { get; set; }

    [Column(TypeName = "int(11)")]
    public int cfhs_abusive { get; set; }

    public double trading_locked { get; set; }

    [Column(TypeName = "int(11)")]
    public int trading_locks_count { get; set; }
}
