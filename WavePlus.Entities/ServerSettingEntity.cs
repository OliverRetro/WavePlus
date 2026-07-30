using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("server_settings")]
public partial class ServerSettingEntity
{
    [Key]
    [Column("key")]
    public string Key { get; set; }

    [Required]
    [Column("value", TypeName = "text")]
    public string Value { get; set; }

    [Required]
    [Column("description", TypeName = "text")]
    public string Description { get; set; }
}