using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("Bantype", Name = "bantype")]
[Index("Value", Name = "value")]
[Index("UserId", Name = "user_id")]
[Table("bans")]
public partial class BanEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Required]
    [Column("bantype", TypeName = "enum('account','ip')")]
    public string Bantype { get; set; }

    [Required]
    [StringLength(50)]
    [Column("value")]
    public string Value { get; set; }

    [Required]
    [Column("reason", TypeName = "text")]
    public string Reason { get; set; }

    [Column("expire")]
    public double Expire { get; set; }

    [Required]
    [StringLength(50)]
    [Column("added_by")]
    public string AddedBy { get; set; }

    [Required]
    [StringLength(50)]
    [Column("added_date")]
    public string AddedDate { get; set; }

    [Required]
    [Column("appeal_state", TypeName = "enum('0','1','2')")]
    public string AppealState { get; set; }
}