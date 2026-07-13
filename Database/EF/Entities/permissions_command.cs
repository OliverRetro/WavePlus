using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("permissions_commands")]
public partial class PermissionsCommand
{
    [Key]
    [StringLength(45)]
    [Column("command")]
    public string Command { get; set; }

    [Column("group_id", TypeName = "int(11)")]
    public int GroupId { get; set; }

    [Column("subscription_id", TypeName = "int(11)")]
    public int SubscriptionId { get; set; }
}
