using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("AiType", Name = "ai_type")]
[Index("Id", Name = "id", IsUnique = true)]
[Index("RoomId", Name = "room_id")]
[Index("UserId", Name = "user_id")]
[Table("bots")]
public partial class BotEntity
{
    [Key]
    [Column("id", TypeName = "int(10) unsigned")]
    public uint Id { get; set; }

    [Column("room_id", TypeName = "int(10) unsigned")]
    public uint RoomId { get; set; }

    [Column("user_id", TypeName = "int(10) unsigned")]
    public uint UserId { get; set; }

    [Required]
    [Column("ai_type", TypeName = "enum('generic','bartender','pet')")]
    public string AiType { get; set; }

    [Required]
    [StringLength(100)]
    [Column("name")]
    public string Name { get; set; }

    [Required]
    [StringLength(120)]
    [Column("motto")]
    public string Motto { get; set; }

    [Required]
    [Column("look", TypeName = "text")]
    public string Look { get; set; }

    [Column("x", TypeName = "int(11)")]
    public int X { get; set; }

    [Column("y", TypeName = "int(11)")]
    public int Y { get; set; }

    [Column("z", TypeName = "int(11)")]
    public int Z { get; set; }

    [Column("rotation", TypeName = "int(11)")]
    public int Rotation { get; set; }

    [Required]
    [Column("walk_mode", TypeName = "enum('stand','freeroam','specified_range')")]
    public string WalkMode { get; set; }

    [Column("min_x", TypeName = "int(11)")]
    public int MinX { get; set; }

    [Column("min_y", TypeName = "int(11)")]
    public int MinY { get; set; }

    [Column("max_x", TypeName = "int(11)")]
    public int MaxX { get; set; }

    [Column("max_y", TypeName = "int(11)")]
    public int MaxY { get; set; }

    [Column("effect", TypeName = "int(2)")]
    public int Effect { get; set; }

    [Required]
    [StringLength(5)]
    [Column("gender")]
    public string Gender { get; set; }

    [Column("dance", TypeName = "int(11)")]
    public int Dance { get; set; }

    [Required]
    [Column("automatic_chat", TypeName = "enum('false','true')")]
    public string AutomaticChat { get; set; }

    [Column("speaking_interval", TypeName = "int(8)")]
    public int SpeakingInterval { get; set; }

    [Required]
    [Column("mix_sentences", TypeName = "enum('0','1')")]
    public string MixSentences { get; set; }

    [Column("chat_bubble", TypeName = "int(11)")]
    public int ChatBubble { get; set; }

    [Column("effect_id", TypeName = "int(11)")]
    public int EffectId { get; set; }
}