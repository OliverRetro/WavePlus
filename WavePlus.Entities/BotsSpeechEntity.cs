using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("bots_speech")]
[Index("BotId", Name = "bot_id")]
public partial class BotsSpeechEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("bot_id", TypeName = "int(10) unsigned")]
    public uint BotId { get; set; }

    [Required]
    [StringLength(200)]
    [Column("text")]
    public string Text { get; set; }

    [Required]
    [Column("shout", TypeName = "enum('0','1')")]
    public string Shout { get; set; }

    [Column("type", TypeName = "enum('normal','rentable')")]
    public string Type { get; set; }
}