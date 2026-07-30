using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("catalog_bot_presets")]
public partial class CatalogBotPresetEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    [Column("name")]
    public string Name { get; set; }

    [Required]
    [StringLength(255)]
    [Column("figure")]
    public string Figure { get; set; }

    [Required]
    [StringLength(255)]
    [Column("gender")]
    public string Gender { get; set; }

    [Required]
    [StringLength(255)]
    [Column("motto")]
    public string Motto { get; set; }

    [Required]
    [Column("ai_type", TypeName = "enum('pet','generic','bartender')")]
    public string AiType { get; set; }
}