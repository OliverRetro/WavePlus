using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("UserId", "Voucher", Name = "user_id, voucher", IsUnique = true)]
[Table("user_vouchers")]
public partial class UserVoucherEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Required]
    [StringLength(45)]
    [Column("voucher")]
    public string Voucher { get; set; }
}