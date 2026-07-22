using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("room_items_moodlight")]
[Index("Enabled", Name = "enabled")]
[Index("ItemId", Name = "item_id")]
public partial class RoomItemsMoodlightEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("item_id", TypeName = "int(10) unsigned")]
    public uint ItemId { get; set; }

    [Required]
    [Column("enabled", TypeName = "enum('0','1')")]
    public string Enabled { get; set; }

    [Column("current_preset", TypeName = "int(11)")]
    public int CurrentPreset { get; set; }

    [Required]
    [Column("preset_one", TypeName = "text")]
    public string PresetOne { get; set; }

    [Required]
    [Column("preset_two", TypeName = "text")]
    public string PresetTwo { get; set; }

    [Required]
    [Column("preset_three", TypeName = "text")]
    public string PresetThree { get; set; }
}