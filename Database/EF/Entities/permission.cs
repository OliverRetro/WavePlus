using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class permission
{
    [Key]
    [Column(TypeName = "int(3)")]
    public int id { get; set; }

    [Required]
    [Column("permission")]
    [StringLength(50)]
    public string permission1 { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string description { get; set; }
}
