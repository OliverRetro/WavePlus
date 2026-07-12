using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("furniture")]
[Index("id", Name = "id", IsUnique = true)]
[Index("sprite_id", Name = "sprite_id")]
public partial class furniture
{
    [Key]
    [Column(TypeName = "int(10) unsigned")]
    public uint id { get; set; }

    [Required]
    [StringLength(70)]
    public string item_name { get; set; }

    [Required]
    [StringLength(56)]
    public string public_name { get; set; }

    [Required]
    [Column(TypeName = "enum('s','i','e','h','v','r','b','p')")]
    public string type { get; set; }

    [Column(TypeName = "int(11)")]
    public int width { get; set; }

    [Column(TypeName = "int(11)")]
    public int length { get; set; }

    public double stack_height { get; set; }

    [Column(TypeName = "tinyint(4)")]
    public sbyte can_stack { get; set; }

    [Column(TypeName = "tinyint(4)")]
    public sbyte can_sit { get; set; }

    [Column(TypeName = "tinyint(4)")]
    public sbyte? is_walkable { get; set; }

    [Column(TypeName = "int(11)")]
    public int? sprite_id { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string allow_recycle { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string allow_trade { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string allow_marketplace_sell { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string allow_gift { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string allow_inventory_stack { get; set; }

    [Required]
    [StringLength(123)]
    public string interaction_type { get; set; }

    [Column(TypeName = "int(11)")]
    public int behaviour_data { get; set; }

    [Column(TypeName = "int(11)")]
    public int interaction_modes_count { get; set; }

    [Required]
    [StringLength(255)]
    public string vending_ids { get; set; }

    [Required]
    [StringLength(50)]
    public string height_adjustable { get; set; }

    [Column(TypeName = "int(11)")]
    public int effect_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int wired_id { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string is_rare { get; set; }

    [Column(TypeName = "int(11)")]
    public int clothing_id { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string extra_rot { get; set; }

    [Column(TypeName = "int(11)")]
    public int? allow_lay { get; set; }
}
