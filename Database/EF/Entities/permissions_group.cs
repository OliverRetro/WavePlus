using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class permissions_group
{
    [Key]
    [Column(TypeName = "int(3)")]
    public int id { get; set; }

    [Required]
    [StringLength(50)]
    public string name { get; set; }

    [Required]
    [StringLength(50)]
    public string description { get; set; }

    [Required]
    [StringLength(12)]
    public string badge_code { get; set; }

    [Column(TypeName = "int(11)")]
    public int level { get; set; }
}
