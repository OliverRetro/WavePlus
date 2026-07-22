using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("BotAi", Name = "bot_id")]
[Table("bots_responses")]
public partial class BotsResponseEntity
{
    [Key]
    [Column("id", TypeName = "int(10) unsigned")]
    public uint Id { get; set; }

    [Required]
    [Column("bot_ai", TypeName = "enum('generic','bartender')")]
    public string BotAi { get; set; }

    [Required]
    [Column("chat_keywords", TypeName = "text")]
    public string ChatKeywords { get; set; }

    [Required]
    [StringLength(200)]
    [Column("response_text")]
    public string ResponseText { get; set; }

    [Required]
    [Column("response_mode", TypeName = "enum('say','shout','whisper')")]
    public string ResponseMode { get; set; }

    [Required]
    [StringLength(25)]
    [Column("response_beverage")]
    public string ResponseBeverage { get; set; }
}