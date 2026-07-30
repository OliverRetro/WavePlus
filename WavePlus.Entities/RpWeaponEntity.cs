using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("rp_weapons")]
public partial class RpWeaponEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [StringLength(55)]
    [Column("name")]
    public string Name { get; set; }

    [Required]
    [StringLength(255)]
    [Column("description")]
    public string Description { get; set; }

    [Required]
    [StringLength(110)]
    [Column("hit_message")]
    public string HitMessage { get; set; }

    [Required]
    [StringLength(110)]
    [Column("critical_hit_message")]
    public string CriticalHitMessage { get; set; }

    [Column("minimum_damage", TypeName = "int(11)")]
    public int MinimumDamage { get; set; }

    [Column("maximum_damage", TypeName = "int(11)")]
    public int MaximumDamage { get; set; }

    [Column("critical_chance", TypeName = "smallint(6)")]
    public short CriticalChance { get; set; }

    [Column("critical_multiplier")]
    public double CriticalMultiplier { get; set; }

    [Column("effect_id", TypeName = "smallint(6)")]
    public short EffectId { get; set; }

    [Column("range", TypeName = "int(11)")]
    public int Range { get; set; }

    [Column("allow_diagonal", TypeName = "tinyint(1)")]
    public bool AllowDiagonal { get; set; }

    [Column("durability", TypeName = "int(11)")]
    public int Durability { get; set; }

    [Required]
    [StringLength(255)]
    [Column("image")]
    public string Image { get; set; }

    [Column("stun_chance", TypeName = "int(11)")]
    public int StunChance { get; set; }

    [Column("rarity", TypeName = "smallint(6)")]
    public short Rarity { get; set; }
}