using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class achievements_talent
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Required]
    [Column(TypeName = "enum('citizenship','status')")]
    public string type { get; set; }

    [Column(TypeName = "int(11)")]
    public int parent_category { get; set; }

    [Column(TypeName = "int(11)")]
    public int level { get; set; }

    [Column(TypeName = "int(11)")]
    public int order_num { get; set; }

    [Required]
    [StringLength(255)]
    public string achievement_group { get; set; }

    [Column(TypeName = "int(11)")]
    public int achievement_level { get; set; }

    [Required]
    [StringLength(255)]
    public string prize { get; set; }

    [Column(TypeName = "int(11) unsigned")]
    public uint prize_baseitem { get; set; }
}
