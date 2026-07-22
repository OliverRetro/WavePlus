using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("rp_weapon_skins")]
public partial class RpWeaponSkinEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("weapon_id", TypeName = "int(11)")]
    public int WeaponId { get; set; }

    [Required]
    [StringLength(255)]
    [Column("name")]
    public string Name { get; set; }

    [Required]
    [StringLength(110)]
    [Column("hit_message")]
    public string HitMessage { get; set; }

    [Required]
    [StringLength(110)]
    [Column("critical_hit_message")]
    public string CriticalHitMessage { get; set; }

    [Column("base_price", TypeName = "int(11)")]
    public int BasePrice { get; set; }

    [Column("rarity", TypeName = "smallint(6)")]
    public short Rarity { get; set; }

    [Column("effect_id", TypeName = "int(11)")]
    public int EffectId { get; set; }
}