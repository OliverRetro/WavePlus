using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("BaseItem", Name = "base_item")]
[Index("Id", Name = "id", IsUnique = true)]
[Index("RoomId", Name = "roomid")]
[Index("UserId", Name = "userid")]
[Table("items")]
public partial class ItemEntity
{
    [Key]
    [Column("id", TypeName = "int(10) unsigned")]
    public uint Id { get; set; }

    [Column("user_id", TypeName = "int(10)")]
    public int UserId { get; set; }

    [Column("room_id", TypeName = "int(10) unsigned")]
    public uint RoomId { get; set; }

    [Column("base_item", TypeName = "int(10) unsigned")]
    public uint BaseItem { get; set; }

    [Required]
    [Column("extra_data", TypeName = "text")]
    public string ExtraData { get; set; }

    [Column("x", TypeName = "int(11)")]
    public int X { get; set; }

    [Column("y", TypeName = "int(11)")]
    public int Y { get; set; }

    [Column("z")]
    public double Z { get; set; }

    [Column("rot", TypeName = "int(11)")]
    public int Rot { get; set; }

    [StringLength(100)]
    [Column("wall_pos")]
    public string WallPos { get; set; }

    [Column("limited_number", TypeName = "int(11)")]
    public int? LimitedNumber { get; set; }

    [Column("limited_stack", TypeName = "int(11)")]
    public int? LimitedStack { get; set; }
}