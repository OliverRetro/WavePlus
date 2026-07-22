using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("RoomId", Name = "room_id")]
[Table("room_taxi_points")]
public partial class RoomTaxiPointEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("room_id", TypeName = "int(11)")]
    public int RoomId { get; set; }

    [Column("x", TypeName = "int(11)")]
    public int X { get; set; }

    [Column("y", TypeName = "int(11)")]
    public int Y { get; set; }

    [Column("z")]
    public double Z { get; set; }

    [Column("rotation", TypeName = "int(11)")]
    public int Rotation { get; set; }

    [Column("cost", TypeName = "int(11)")]
    public int Cost { get; set; }

    [Column("delay", TypeName = "int(11)")]
    public int Delay { get; set; }
}