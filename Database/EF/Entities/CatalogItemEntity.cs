using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("ItemId", Name = "item_ids")]
[Table("catalog_items")]
public partial class CatalogItemEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("page_id", TypeName = "int(11)")]
    public int PageId { get; set; }

    [Required]
    [StringLength(120)]
    [Column("item_id")]
    public string ItemId { get; set; }

    [Required]
    [StringLength(100)]
    [Column("catalog_name")]
    public string CatalogName { get; set; }

    [Column("cost_credits", TypeName = "int(11)")]
    public int CostCredits { get; set; }

    [Column("cost_pixels", TypeName = "int(11)")]
    public int CostPixels { get; set; }

    [Column("cost_diamonds", TypeName = "int(11)")]
    public int CostDiamonds { get; set; }

    [Column("amount", TypeName = "int(11)")]
    public int Amount { get; set; }

    [Column("limited_sells", TypeName = "int(11)")]
    public int LimitedSells { get; set; }

    [Column("limited_stack", TypeName = "int(11)")]
    public int LimitedStack { get; set; }

    [Required]
    [Column("offer_active", TypeName = "enum('0','1')")]
    public string OfferActive { get; set; }

    [Required]
    [StringLength(255)]
    [Column("extradata")]
    public string Extradata { get; set; }

    [Required]
    [StringLength(5)]
    [Column("badge")]
    public string Badge { get; set; }

    [Column("offer_id", TypeName = "int(11)")]
    public int OfferId { get; set; }

    [Column("points_type", TypeName = "int(11)")]
    public int? PointsType { get; set; }
}