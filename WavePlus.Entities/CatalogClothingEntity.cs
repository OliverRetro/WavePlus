using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("catalog_clothing")]
public partial class CatalogClothingEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [StringLength(55)]
    [Column("clothing_name")]
    public string ClothingName { get; set; }

    [Required]
    [StringLength(85)]
    [Column("clothing_parts")]
    public string ClothingParts { get; set; }
}