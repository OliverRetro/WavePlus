using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("bots_pet_responses")]
public partial class BotsPetResponseEntity
{
    [Key]
    [Column("pet_id")]
    public string PetId { get; set; }

    [Required]
    [Column("responses", TypeName = "text")]
    public string Responses { get; set; }
}