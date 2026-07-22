using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("permissions_subscriptions")]
public partial class PermissionsSubscriptionEntity
{
    [Key]
    [Column("id", TypeName = "int(10)")]
    public int Id { get; set; }

    [Column("subscription_id", TypeName = "int(10)")]
    public int SubscriptionId { get; set; }

    [Column("permission_id", TypeName = "int(10)")]
    public int PermissionId { get; set; }
}