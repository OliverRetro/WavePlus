using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("messenger_offline_messages")]
public partial class MessengerOfflineMessageEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("to_id", TypeName = "int(11) unsigned")]
    public uint ToId { get; set; }

    [Column("from_id", TypeName = "int(11) unsigned")]
    public uint FromId { get; set; }

    [Required]
    [StringLength(255)]
    [Column("message")]
    public string Message { get; set; }

    [Column("timestamp")]
    public double Timestamp { get; set; }
}