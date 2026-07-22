using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("subscriptions")]
public partial class SubscriptionEntity
{
    [Key]
    [Column("id", TypeName = "int(10)")]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    [Column("name")]
    public string Name { get; set; }

    [Required]
    [StringLength(10)]
    [Column("badge_code")]
    public string BadgeCode { get; set; }

    [Column("credits", TypeName = "int(11)")]
    public int Credits { get; set; }

    [Column("duckets", TypeName = "int(11)")]
    public int Duckets { get; set; }

    [Column("respects", TypeName = "int(11)")]
    public int Respects { get; set; }

    [Column("static_aggression", TypeName = "smallint(6)")]
    public short StaticAggression { get; set; }

    [Column("trash_search_cooldown", TypeName = "smallint(6)")]
    public short TrashSearchCooldown { get; set; }
}