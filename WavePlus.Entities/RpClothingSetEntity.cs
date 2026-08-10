using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("rp_clothing_sets")]
public partial class RpClothingSetEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("tab_id", TypeName = "smallint(6)")]
    public short? TabId { get; set; }

    [Required]
    [StringLength(32)]
    [Column("set_type")]
    public string SetType { get; set; }

    [Required]
    [StringLength(32)]
    [Column("part_types")]
    public string PartTypes { get; set; }

    [Required]
    [StringLength(32)]
    [Column("color")]
    public string Color { get; set; }

    [Required]
    [StringLength(80)]
    [Column("name")]
    public string Name { get; set; }

    [Column("stock", TypeName = "int(11)")]
    public int Stock { get; set; }

    [Column("base_price", TypeName = "int(11)")]
    public int BasePrice { get; set; }

    [Column("discount_price", TypeName = "int(11)")]
    public int DiscountPrice { get; set; }

    [Column("visible", TypeName = "tinyint(1)")]
    public bool Visible { get; set; }
}