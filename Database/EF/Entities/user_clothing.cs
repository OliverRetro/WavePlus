using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("user_clothing")]
[Index("user_id", Name = "user_id")]
public partial class user_clothing
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int user_id { get; set; }

    [Required]
    [StringLength(25)]
    public string part_id { get; set; }

    [Required]
    [StringLength(45)]
    public string part { get; set; }
}
