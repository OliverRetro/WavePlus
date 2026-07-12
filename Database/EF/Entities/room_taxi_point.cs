using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("room_id", Name = "room_id")]
public partial class room_taxi_point
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int room_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int x { get; set; }

    [Column(TypeName = "int(11)")]
    public int y { get; set; }

    public double z { get; set; }

    [Column(TypeName = "int(11)")]
    public int rotation { get; set; }

    [Column(TypeName = "int(11)")]
    public int cost { get; set; }

    [Column(TypeName = "int(11)")]
    public int delay { get; set; }
}
