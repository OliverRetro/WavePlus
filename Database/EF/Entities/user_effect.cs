using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("user_id", Name = "user_id")]
public partial class user_effect
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint? user_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int? effect_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int? total_duration { get; set; }

    [Column(TypeName = "enum('0','1')")]
    public string is_activated { get; set; }

    public double? activated_stamp { get; set; }

    [Column(TypeName = "int(11)")]
    public int? quantity { get; set; }
}
