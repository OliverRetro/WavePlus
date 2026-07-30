using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Keyless]
[Table("catalog_pet_races")]
public partial class CatalogPetRaceEntity
{
    [Column("raceid", TypeName = "int(255)")]
    public int? Raceid { get; set; }

    [Column("color1", TypeName = "int(255)")]
    public int? Color1 { get; set; }

    [Column("color2", TypeName = "int(255)")]
    public int? Color2 { get; set; }

    [Column("has1color", TypeName = "enum('1','0')")]
    public string Has1color { get; set; }

    [Column("has2color", TypeName = "enum('1','0')")]
    public string Has2color { get; set; }
}