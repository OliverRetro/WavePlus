using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("rp_items")]
public partial class RpItemEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [StringLength(55)]
    [Column("item_name")]
    public string ItemName { get; set; }

    [Required]
    [Column("handitem_data")]
    public string HanditemData { get; set; }

    [Required]
    [Column("effect_data")]
    public string EffectData { get; set; }

    [Column("base_price", TypeName = "int(11)")]
    public int BasePrice { get; set; }

    [Required]
    [Column("attribute_effects")]
    public string AttributeEffects { get; set; }

    [Required]
    [StringLength(255)]
    [Column("image_url")]
    public string ImageUrl { get; set; }

    [Column("stack_limit", TypeName = "int(11)")]
    public int StackLimit { get; set; }

    [Required]
    [StringLength(32)]
    [Column("item_type")]
    public string ItemType { get; set; }

    [Column("rarity", TypeName = "smallint(6)")]
    public short Rarity { get; set; }

    [Required]
    [StringLength(255)]
    [Column("bubble")]
    public string Bubble { get; set; }

    [Required]
    [Column("clothing")]
    public string Clothing { get; set; }
}