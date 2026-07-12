using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class moderation_preset
{
    [Key]
    [Column(TypeName = "int(10) unsigned")]
    public uint id { get; set; }

    [Required]
    [Column(TypeName = "enum('user','room')")]
    public string type { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string message { get; set; }

    [Column(TypeName = "int(11)")]
    public int? enabled { get; set; }
}
