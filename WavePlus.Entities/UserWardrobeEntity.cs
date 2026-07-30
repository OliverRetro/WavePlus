using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("user_wardrobe")]
[Index("SlotId", Name = "slot_id")]
[Index("UserId", Name = "user_id")]
public partial class UserWardrobeEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(10) unsigned")]
    public uint UserId { get; set; }

    [Column("slot_id", TypeName = "int(10) unsigned")]
    public uint SlotId { get; set; }

    [Required]
    [StringLength(120)]
    [Column("look")]
    public string Look { get; set; }

    [Required]
    [Column("gender", TypeName = "enum('F','M')")]
    public string Gender { get; set; }
}