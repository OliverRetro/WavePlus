using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("moderation_topics")]
public partial class ModerationTopicEntity
{
    [Key]
    [Column("id", TypeName = "int(11) unsigned")]
    public uint Id { get; set; }

    [Required]
    [StringLength(225)]
    [Column("caption")]
    public string Caption { get; set; }
}