using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("ai_type", Name = "ai_type")]
[Index("id", Name = "id", IsUnique = true)]
[Index("room_id", Name = "room_id")]
[Index("user_id", Name = "user_id")]
public partial class bot
{
    [Key]
    [Column(TypeName = "int(10) unsigned")]
    public uint id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint room_id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint user_id { get; set; }

    [Required]
    [Column(TypeName = "enum('generic','bartender','pet')")]
    public string ai_type { get; set; }

    [Required]
    [StringLength(100)]
    public string name { get; set; }

    [Required]
    [StringLength(120)]
    public string motto { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string look { get; set; }

    [Column(TypeName = "int(11)")]
    public int x { get; set; }

    [Column(TypeName = "int(11)")]
    public int y { get; set; }

    [Column(TypeName = "int(11)")]
    public int z { get; set; }

    [Column(TypeName = "int(11)")]
    public int rotation { get; set; }

    [Required]
    [Column(TypeName = "enum('stand','freeroam','specified_range')")]
    public string walk_mode { get; set; }

    [Column(TypeName = "int(11)")]
    public int min_x { get; set; }

    [Column(TypeName = "int(11)")]
    public int min_y { get; set; }

    [Column(TypeName = "int(11)")]
    public int max_x { get; set; }

    [Column(TypeName = "int(11)")]
    public int max_y { get; set; }

    [Column(TypeName = "int(2)")]
    public int effect { get; set; }

    [Required]
    [StringLength(5)]
    public string gender { get; set; }

    [Column(TypeName = "int(11)")]
    public int dance { get; set; }

    [Required]
    [Column(TypeName = "enum('false','true')")]
    public string automatic_chat { get; set; }

    [Column(TypeName = "int(8)")]
    public int speaking_interval { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string mix_sentences { get; set; }

    [Column(TypeName = "int(11)")]
    public int chat_bubble { get; set; }

    [Column(TypeName = "int(11)")]
    public int effect_id { get; set; }
}
