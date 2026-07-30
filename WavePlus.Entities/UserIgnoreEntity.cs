using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("IgnoreId", Name = "ignore_id")]
[Index("UserId", Name = "user_id")]
[Table("user_ignores")]
public partial class UserIgnoreEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(10) unsigned")]
    public uint UserId { get; set; }

    [Column("ignore_id", TypeName = "int(10) unsigned")]
    public uint IgnoreId { get; set; }
}