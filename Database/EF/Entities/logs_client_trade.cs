using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("logs_client_trade")]
public partial class logs_client_trade
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column("1id", TypeName = "int(11)")]
    public int? _1id { get; set; }

    [Column("2id", TypeName = "int(11)")]
    public int? _2id { get; set; }

    [Column("1items", TypeName = "text")]
    public string _1items { get; set; }

    [Column("2items", TypeName = "text")]
    public string _2items { get; set; }

    [StringLength(20)]
    public string timestamp { get; set; }
}
