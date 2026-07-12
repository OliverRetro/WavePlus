using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("bots_speech")]
[Index("bot_id", Name = "bot_id")]
public partial class bots_speech
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint bot_id { get; set; }

    [Required]
    [StringLength(200)]
    public string text { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string shout { get; set; }

    [Column(TypeName = "enum('normal','rentable')")]
    public string type { get; set; }
}
