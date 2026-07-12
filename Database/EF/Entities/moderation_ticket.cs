using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("id", Name = "id", IsUnique = true)]
[Index("moderator_id", Name = "moderator_id")]
[Index("reported_id", Name = "reported_id")]
[Index("sender_id", Name = "sender_id")]
[Index("status", Name = "status")]
public partial class moderation_ticket
{
    [Key]
    [Column(TypeName = "int(10) unsigned")]
    public uint id { get; set; }

    [Column(TypeName = "int(11)")]
    public int score { get; set; }

    [Column(TypeName = "int(11)")]
    public int type { get; set; }

    [Required]
    [Column(TypeName = "enum('open','picked','resolved','abusive','invalid','deleted')")]
    public string status { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint sender_id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint reported_id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint moderator_id { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string message { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint room_id { get; set; }

    [Required]
    [StringLength(100)]
    public string room_name { get; set; }

    public double timestamp { get; set; }
}
