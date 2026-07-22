using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("logs_client_staff")]
public partial class LogsClientStaffEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Required]
    [Column("data_string", TypeName = "text")]
    public string DataString { get; set; }

    [Required]
    [StringLength(75)]
    [Column("machine_id")]
    public string MachineId { get; set; }

    [Column("timestamp")]
    public double Timestamp { get; set; }
}