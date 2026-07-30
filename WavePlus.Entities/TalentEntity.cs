using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("talents")]
public partial class TalentEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [Column("type", TypeName = "enum('citizenship','helper')")]
    public string Type { get; set; }

    [Column("level", TypeName = "int(11)")]
    public int? Level { get; set; }

    [Required]
    [Column("data_actions", TypeName = "text")]
    public string DataActions { get; set; }

    [Required]
    [Column("data_gifts", TypeName = "text")]
    public string DataGifts { get; set; }
}