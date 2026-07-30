using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("user_rp_bank")]
[Index("UserId", Name = "uniq_user_rp_bank_user_id", IsUnique = true)]
public partial class UserRpBankEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Column("balance", TypeName = "int(11)")]
    public int Balance { get; set; }

    [Column("account_opened", TypeName = "int(11)")]
    public int AccountOpened { get; set; }
}