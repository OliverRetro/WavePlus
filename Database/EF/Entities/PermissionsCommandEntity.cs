using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("permissions_commands")]
public partial class PermissionsCommandEntity
{
    [Key]
    [StringLength(45)]
    [Column("command")]
    public string Command { get; set; }

    [Column("group_id", TypeName = "int(11)")]
    public int GroupId { get; set; }

    [Column("subscription_id", TypeName = "int(11)")]
    public int SubscriptionId { get; set; }

    /// <summary>
    /// Optional semicolon-separated list of chat aliases that trigger this command
    /// (e.g. "givebubble;addbubble;ab"). Lets aliases be added in the DB without editing code.
    /// </summary>
    [Column("keys")]
    public string Keys { get; set; }
}
