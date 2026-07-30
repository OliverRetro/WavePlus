using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("bots_pet_commands")]
public partial class BotsPetCommandEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    [Column("input_title")]
    public string InputTitle { get; set; }

    [Column("input", TypeName = "text")]
    public string Input { get; set; }
}