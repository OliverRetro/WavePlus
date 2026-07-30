using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("catalog_marketplace_data")]
public partial class CatalogMarketplaceDatumEntity
{
    [Key]
    [Column("id", TypeName = "int(12)")]
    public int Id { get; set; }

    [Column("sprite", TypeName = "int(7)")]
    public int Sprite { get; set; }

    [Column("sold", TypeName = "int(7)")]
    public int Sold { get; set; }

    [Column("avgprice", TypeName = "int(9)")]
    public int Avgprice { get; set; }
}