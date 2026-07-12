using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class rp_skill
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Required]
    [StringLength(25)]
    public string name { get; set; }

    [Required]
    [StringLength(25)]
    public string progress_category { get; set; }

    [Required]
    [StringLength(255)]
    public string description { get; set; }

    [StringLength(255)]
    public string badge_code { get; set; }
}
