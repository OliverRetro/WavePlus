using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("chatlogs_console")]
[Index("FromId", Name = "from_id")]
[Index("Timestamp", Name = "timestamp")]
[Index("ToId", Name = "to_id")]
public partial class ChatlogsConsoleEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("from_id", TypeName = "int(11) unsigned")]
    public uint FromId { get; set; }

    [Column("to_id", TypeName = "int(11) unsigned")]
    public uint ToId { get; set; }

    [Required]
    [Column("message", TypeName = "text")]
    public string Message { get; set; }

    [Column("timestamp")]
    public double Timestamp { get; set; }
}