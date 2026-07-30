using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("logs_client_trade")]
public partial class LogsClientTradeEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("1id", TypeName = "int(11)")]
    public int? _1id { get; set; }

    [Column("2id", TypeName = "int(11)")]
    public int? _2id { get; set; }

    [Column("1items", TypeName = "text")]
    public string _1items { get; set; }

    [Column("2items", TypeName = "text")]
    public string _2items { get; set; }

    [StringLength(20)]
    [Column("timestamp")]
    public string Timestamp { get; set; }
}