using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("room_items_toner")]
[Index("Enabled", Name = "enabled")]
[Index("Id", Name = "id", IsUnique = true)]
public partial class RoomItemsTonerEntity
{
    [Key]
    [Column("id", TypeName = "int(11) unsigned")]
    public uint Id { get; set; }

    [Column("enabled", TypeName = "enum('0','1')")]
    public string Enabled { get; set; }

    [Column("data1", TypeName = "int(11)")]
    public int Data1 { get; set; }

    [Column("data2", TypeName = "int(11)")]
    public int Data2 { get; set; }

    [Column("data3", TypeName = "int(11)")]
    public int Data3 { get; set; }
}