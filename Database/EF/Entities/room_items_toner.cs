using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("room_items_toner")]
[Index("enabled", Name = "enabled")]
[Index("id", Name = "id", IsUnique = true)]
public partial class room_items_toner
{
    [Key]
    [Column(TypeName = "int(11) unsigned")]
    public uint id { get; set; }

    [Column(TypeName = "enum('0','1')")]
    public string enabled { get; set; }

    [Column(TypeName = "int(11)")]
    public int data1 { get; set; }

    [Column(TypeName = "int(11)")]
    public int data2 { get; set; }

    [Column(TypeName = "int(11)")]
    public int data3 { get; set; }
}
