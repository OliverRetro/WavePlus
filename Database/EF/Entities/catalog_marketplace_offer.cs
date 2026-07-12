using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class catalog_marketplace_offer
{
    [Key]
    [Column(TypeName = "int(10) unsigned")]
    public uint offer_id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint item_id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint user_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int asking_price { get; set; }

    [Column(TypeName = "int(11)")]
    public int total_price { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string public_name { get; set; }

    [Column(TypeName = "int(11)")]
    public int sprite_id { get; set; }

    [Required]
    [Column(TypeName = "enum('1','2')")]
    public string item_type { get; set; }

    public double timestamp { get; set; }

    [Required]
    [Column(TypeName = "enum('1','2')")]
    public string state { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string extra_data { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint furni_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int limited_number { get; set; }

    [Column(TypeName = "int(11)")]
    public int limited_stack { get; set; }
}
