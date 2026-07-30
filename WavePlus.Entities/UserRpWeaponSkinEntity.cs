using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("SkinId", Name = "idx_user_rp_weapon_skins_skin_id")]
[Index("UserId", Name = "idx_user_rp_weapon_skins_user_id")]
[Table("user_rp_weapon_skins")]
public partial class UserRpWeaponSkinEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Column("skin_id", TypeName = "int(11)")]
    public int SkinId { get; set; }

    [Column("equipped", TypeName = "int(11)")]
    public int Equipped { get; set; }
}