using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("user_id", Name = "idx_user_rp_weapons_user_id")]
[Index("weapon_id", Name = "idx_user_rp_weapons_weapon_id")]
public partial class user_rp_weapon
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int user_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int weapon_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int durability_left { get; set; }

    [Column(TypeName = "int(11)")]
    public int slot { get; set; }
}
