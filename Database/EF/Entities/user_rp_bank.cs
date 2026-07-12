using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("user_rp_bank")]
[Index("user_id", Name = "uniq_user_rp_bank_user_id", IsUnique = true)]
public partial class user_rp_bank
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int user_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int balance { get; set; }

    [MaxLength(6)]
    public DateTime account_opened { get; set; }
}
