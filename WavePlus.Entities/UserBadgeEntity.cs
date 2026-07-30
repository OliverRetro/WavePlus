using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("UserId", Name = "user_id")]
[Index("UserId", "BadgeId", Name = "user_id, badge_id", IsUnique = true)]
[Table("user_badges")]
public partial class UserBadgeEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(10) unsigned")]
    public uint UserId { get; set; }

    [Required]
    [StringLength(100)]
    [Column("badge_id")]
    public string BadgeId { get; set; }

    [Column("badge_slot", TypeName = "int(11)")]
    public int BadgeSlot { get; set; }
}