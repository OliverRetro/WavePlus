using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("id", Name = "id", IsUnique = true)]
public partial class room_model
{
    [Key]
    [StringLength(100)]
    public string id { get; set; }

    [Column(TypeName = "int(11)")]
    public int door_x { get; set; }

    [Column(TypeName = "int(11)")]
    public int door_y { get; set; }

    public double door_z { get; set; }

    [Column(TypeName = "int(4)")]
    public int door_dir { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string heightmap { get; set; }

    [Required]
    [StringLength(556)]
    public string public_items { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string club_only { get; set; }

    [Required]
    [StringLength(100)]
    public string poolmap { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string custom { get; set; }

    [Column(TypeName = "int(11)")]
    public int wall_height { get; set; }
}
