using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("id", Name = "id", IsUnique = true)]
[Index("owner_id", Name = "owner")]
[Index("room_id", Name = "room_id")]
public partial class group
{
    [Key]
    [Column(TypeName = "int(11) unsigned")]
    public uint id { get; set; }

    [Required]
    [StringLength(50)]
    public string name { get; set; }

    [Required]
    [StringLength(255)]
    public string desc { get; set; }

    [Required]
    [StringLength(50)]
    public string badge { get; set; }

    [Column(TypeName = "int(11) unsigned")]
    public uint owner_id { get; set; }

    [Column(TypeName = "int(50)")]
    public int created { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint room_id { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1','2')")]
    public string state { get; set; }

    [Column(TypeName = "int(11)")]
    public int colour1 { get; set; }

    [Column(TypeName = "int(11)")]
    public int colour2 { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string admindeco { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string forum_enabled { get; set; }

    [Column(TypeName = "smallint(6)")]
    public short group_type { get; set; }
}
