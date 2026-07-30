using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("moderation_presets")]
public partial class ModerationPresetEntity
{
    [Key]
    [Column("id", TypeName = "int(10) unsigned")]
    public uint Id { get; set; }

    [Required]
    [Column("type", TypeName = "enum('user','room')")]
    public string Type { get; set; }

    [Required]
    [Column("message", TypeName = "text")]
    public string Message { get; set; }

    [Column("enabled", TypeName = "int(11)")]
    public int? Enabled { get; set; }
}