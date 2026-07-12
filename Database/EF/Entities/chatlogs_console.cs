using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("chatlogs_console")]
[Index("from_id", Name = "from_id")]
[Index("timestamp", Name = "timestamp")]
[Index("to_id", Name = "to_id")]
public partial class chatlogs_console
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11) unsigned")]
    public uint from_id { get; set; }

    [Column(TypeName = "int(11) unsigned")]
    public uint to_id { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string message { get; set; }

    public double timestamp { get; set; }
}
