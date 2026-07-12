using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("item_id", Name = "item_id")]
public partial class user_present
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint item_id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint base_id { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string extra_data { get; set; }
}
