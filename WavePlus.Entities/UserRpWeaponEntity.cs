using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("UserId", Name = "idx_user_rp_weapons_user_id")]
[Index("WeaponId", Name = "idx_user_rp_weapons_weapon_id")]
[Table("user_rp_weapons")]
public partial class UserRpWeaponEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Column("weapon_id", TypeName = "int(11)")]
    public int WeaponId { get; set; }

    [Column("durability_left", TypeName = "int(11)")]
    public int DurabilityLeft { get; set; }

    [Column("slot", TypeName = "int(11)")]
    public int Slot { get; set; }
}