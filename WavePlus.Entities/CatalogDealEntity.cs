using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("catalog_deals")]
public partial class CatalogDealEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [Column("items", TypeName = "text")]
    public string Items { get; set; }

    [Required]
    [StringLength(35)]
    [Column("name")]
    public string Name { get; set; }

    [Column("room_id", TypeName = "int(11)")]
    public int RoomId { get; set; }
}