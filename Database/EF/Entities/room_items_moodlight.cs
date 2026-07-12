using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("room_items_moodlight")]
[Index("enabled", Name = "enabled")]
[Index("item_id", Name = "item_id")]
public partial class room_items_moodlight
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint item_id { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string enabled { get; set; }

    [Column(TypeName = "int(11)")]
    public int current_preset { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string preset_one { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string preset_two { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string preset_three { get; set; }
}
