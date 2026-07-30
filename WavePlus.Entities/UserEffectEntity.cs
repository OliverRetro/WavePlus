using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("UserId", Name = "user_id")]
[Table("user_effects")]
public partial class UserEffectEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(10) unsigned")]
    public uint? UserId { get; set; }

    [Column("effect_id", TypeName = "int(11)")]
    public int? EffectId { get; set; }

    [Column("total_duration", TypeName = "int(11)")]
    public int? TotalDuration { get; set; }

    [Column("is_activated", TypeName = "enum('0','1')")]
    public string IsActivated { get; set; }

    [Column("activated_stamp")]
    public double? ActivatedStamp { get; set; }

    [Column("quantity", TypeName = "int(11)")]
    public int? Quantity { get; set; }
}