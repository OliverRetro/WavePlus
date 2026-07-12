using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("item_id", Name = "item_ids")]
public partial class catalog_item
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int page_id { get; set; }

    [Required]
    [StringLength(120)]
    public string item_id { get; set; }

    [Required]
    [StringLength(100)]
    public string catalog_name { get; set; }

    [Column(TypeName = "int(11)")]
    public int cost_credits { get; set; }

    [Column(TypeName = "int(11)")]
    public int cost_pixels { get; set; }

    [Column(TypeName = "int(11)")]
    public int cost_diamonds { get; set; }

    [Column(TypeName = "int(11)")]
    public int amount { get; set; }

    [Column(TypeName = "int(11)")]
    public int limited_sells { get; set; }

    [Column(TypeName = "int(11)")]
    public int limited_stack { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string offer_active { get; set; }

    [Required]
    [StringLength(255)]
    public string extradata { get; set; }

    [Required]
    [StringLength(5)]
    public string badge { get; set; }

    [Column(TypeName = "int(11)")]
    public int offer_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int? points_type { get; set; }
}
