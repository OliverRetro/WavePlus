using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("server_locale")]
public partial class ServerLocaleEntity
{
    [Key]
    [Column("key")]
    public string Key { get; set; }

    [Column("value", TypeName = "text")]
    public string Value { get; set; }
}