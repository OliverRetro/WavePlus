using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("base_item", Name = "base_item")]
[Index("id", Name = "id", IsUnique = true)]
[Index("room_id", Name = "roomid")]
[Index("user_id", Name = "userid")]
public partial class item
{
    [Key]
    [Column(TypeName = "int(10) unsigned")]
    public uint id { get; set; }

    [Column(TypeName = "int(10)")]
    public int user_id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint room_id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint base_item { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string extra_data { get; set; }

    [Column(TypeName = "int(11)")]
    public int x { get; set; }

    [Column(TypeName = "int(11)")]
    public int y { get; set; }

    public double z { get; set; }

    [Column(TypeName = "int(11)")]
    public int rot { get; set; }

    [StringLength(100)]
    public string wall_pos { get; set; }

    [Column(TypeName = "int(11)")]
    public int? limited_number { get; set; }

    [Column(TypeName = "int(11)")]
    public int? limited_stack { get; set; }
}
