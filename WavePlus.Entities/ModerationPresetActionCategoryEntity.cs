using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("moderation_preset_action_categories")]
public partial class ModerationPresetActionCategoryEntity
{
    [Key]
    [Column("id", TypeName = "int(10) unsigned")]
    public uint Id { get; set; }

    [Required]
    [StringLength(32)]
    [Column("caption")]
    public string Caption { get; set; }
}