using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("catalog_promotions")]
public partial class CatalogPromotionEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [StringLength(35)]
    [Column("title")]
    public string Title { get; set; }

    [StringLength(75)]
    [Column("image")]
    public string Image { get; set; }

    [Column("unknown", TypeName = "int(11)")]
    public int? Unknown { get; set; }

    [StringLength(35)]
    [Column("page_link")]
    public string PageLink { get; set; }

    [Column("parent_id", TypeName = "int(11)")]
    public int? ParentId { get; set; }
}