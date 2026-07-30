using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("logs_client_namechange")]
public partial class LogsClientNamechangeEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Required]
    [StringLength(50)]
    [Column("new_name")]
    public string NewName { get; set; }

    [Required]
    [StringLength(50)]
    [Column("old_name")]
    public string OldName { get; set; }

    [Column("timestamp", TypeName = "int(11)")]
    public int Timestamp { get; set; }
}