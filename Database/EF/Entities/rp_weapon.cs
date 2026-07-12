using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class rp_weapon
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Required]
    [StringLength(55)]
    public string name { get; set; }

    [Required]
    [StringLength(255)]
    public string description { get; set; }

    /// <summary>
    /// Variables available username and damage
    /// </summary>
    [Required]
    [StringLength(110)]
    public string hit_message { get; set; }

    [Required]
    [StringLength(110)]
    public string critical_hit_message { get; set; }

    [Column(TypeName = "int(11)")]
    public int minimum_damage { get; set; }

    [Column(TypeName = "int(11)")]
    public int maximum_damage { get; set; }

    /// <summary>
    /// As %. For instance 50% is 50-50 on hitting it, without Skill boosts.
    /// </summary>
    [Column(TypeName = "smallint(6)")]
    public short critical_chance { get; set; }

    public double critical_multiplier { get; set; }

    [Column(TypeName = "smallint(6)")]
    public short effect_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int range { get; set; }

    [Column(TypeName = "int(11)")]
    public int durability { get; set; }

    [Required]
    [StringLength(255)]
    public string image { get; set; }

    /// <summary>
    /// 0-100
    /// </summary>
    [Column(TypeName = "int(11)")]
    public int stun_chance { get; set; }

    [Column(TypeName = "smallint(6)")]
    public short rarity { get; set; }
}
