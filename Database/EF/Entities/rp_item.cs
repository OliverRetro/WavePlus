using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class rp_item
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Required]
    [StringLength(55)]
    public string item_name { get; set; }

    [Required]
    public string handitem_data { get; set; }

    [Required]
    public string effect_data { get; set; }

    [Column(TypeName = "int(11)")]
    public int base_price { get; set; }

    [Required]
    public string attribute_effects { get; set; }

    [Required]
    [StringLength(255)]
    public string image_url { get; set; }

    [Column(TypeName = "int(11)")]
    public int stack_limit { get; set; }

    [Required]
    [StringLength(32)]
    public string item_type { get; set; }
}
