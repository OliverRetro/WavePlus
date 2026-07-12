using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("skin_id", Name = "idx_user_rp_weapon_skins_skin_id")]
[Index("user_id", Name = "idx_user_rp_weapon_skins_user_id")]
public partial class user_rp_weapon_skin
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int user_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int skin_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int equipped { get; set; }
}
