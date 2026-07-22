using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("Id", Name = "id", IsUnique = true)]
[Table("room_models")]
public partial class RoomModelEntity
{
    [Key]
    [StringLength(100)]
    [Column("id")]
    public string Id { get; set; }

    [Column("door_x", TypeName = "int(11)")]
    public int DoorX { get; set; }

    [Column("door_y", TypeName = "int(11)")]
    public int DoorY { get; set; }

    [Column("door_z")]
    public double DoorZ { get; set; }

    [Column("door_dir", TypeName = "int(4)")]
    public int DoorDir { get; set; }

    [Required]
    [Column("heightmap", TypeName = "text")]
    public string Heightmap { get; set; }

    [Required]
    [StringLength(556)]
    [Column("public_items")]
    public string PublicItems { get; set; }

    [Required]
    [Column("club_only", TypeName = "enum('0','1')")]
    public string ClubOnly { get; set; }

    [Required]
    [StringLength(100)]
    [Column("poolmap")]
    public string Poolmap { get; set; }

    [Required]
    [Column("custom", TypeName = "enum('0','1')")]
    public string Custom { get; set; }

    [Column("wall_height", TypeName = "int(11)")]
    public int WallHeight { get; set; }
}