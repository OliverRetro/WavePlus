using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class rp_weapon_skin
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int weapon_id { get; set; }

    [Required]
    [StringLength(255)]
    public string name { get; set; }

    [Required]
    [StringLength(110)]
    public string hit_message { get; set; }

    [Required]
    [StringLength(110)]
    public string critical_hit_message { get; set; }

    [Column(TypeName = "int(11)")]
    public int base_price { get; set; }

    [Column(TypeName = "smallint(6)")]
    public short rarity { get; set; }

    [Column(TypeName = "int(11)")]
    public int effect_id { get; set; }
}
