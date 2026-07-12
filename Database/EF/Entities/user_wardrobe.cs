using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("user_wardrobe")]
[Index("slot_id", Name = "slot_id")]
[Index("user_id", Name = "user_id")]
public partial class user_wardrobe
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint user_id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint slot_id { get; set; }

    [Required]
    [StringLength(120)]
    public string look { get; set; }

    [Required]
    [Column(TypeName = "enum('F','M')")]
    public string gender { get; set; }
}
