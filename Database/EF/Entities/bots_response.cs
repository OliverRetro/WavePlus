using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("bot_ai", Name = "bot_id")]
public partial class bots_response
{
    [Key]
    [Column(TypeName = "int(10) unsigned")]
    public uint id { get; set; }

    [Required]
    [Column(TypeName = "enum('generic','bartender')")]
    public string bot_ai { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string chat_keywords { get; set; }

    [Required]
    [StringLength(200)]
    public string response_text { get; set; }

    [Required]
    [Column(TypeName = "enum('say','shout','whisper')")]
    public string response_mode { get; set; }

    [Required]
    [StringLength(25)]
    public string response_beverage { get; set; }
}
