using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("target", Name = "target")]
[Index("type", Name = "type")]
[Index("user_id", Name = "user_id")]
public partial class user_relationship
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int user_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int target { get; set; }

    [Required]
    [Column(TypeName = "enum('1','2','3')")]
    public string type { get; set; }
}
