using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("catalog_marketplace_offers")]
public partial class CatalogMarketplaceOfferEntity
{
    [Key]
    [Column("offer_id", TypeName = "int(10) unsigned")]
    public uint OfferId { get; set; }

    [Column("item_id", TypeName = "int(10) unsigned")]
    public uint ItemId { get; set; }

    [Column("user_id", TypeName = "int(10) unsigned")]
    public uint UserId { get; set; }

    [Column("asking_price", TypeName = "int(11)")]
    public int AskingPrice { get; set; }

    [Column("total_price", TypeName = "int(11)")]
    public int TotalPrice { get; set; }

    [Required]
    [Column("public_name", TypeName = "text")]
    public string PublicName { get; set; }

    [Column("sprite_id", TypeName = "int(11)")]
    public int SpriteId { get; set; }

    [Required]
    [Column("item_type", TypeName = "enum('1','2')")]
    public string ItemType { get; set; }

    [Column("timestamp")]
    public double Timestamp { get; set; }

    [Required]
    [Column("state", TypeName = "enum('1','2')")]
    public string State { get; set; }

    [Required]
    [Column("extra_data", TypeName = "text")]
    public string ExtraData { get; set; }

    [Column("furni_id", TypeName = "int(10) unsigned")]
    public uint FurniId { get; set; }

    [Column("limited_number", TypeName = "int(11)")]
    public int LimitedNumber { get; set; }

    [Column("limited_stack", TypeName = "int(11)")]
    public int LimitedStack { get; set; }
}