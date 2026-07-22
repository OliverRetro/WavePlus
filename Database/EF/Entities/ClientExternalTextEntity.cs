using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("client_external_texts")]
public partial class ClientExternalTextEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [StringLength(70)]
    [Column("key")]
    public string Key { get; set; }

    [Required]
    [Column("value", TypeName = "text")]
    public string Value { get; set; }
}