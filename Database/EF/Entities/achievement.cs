using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class achievement
{
    [Key]
    [Column(TypeName = "int(10) unsigned")]
    public uint id { get; set; }

    [Required]
    [StringLength(64)]
    public string group_name { get; set; }

    [Required]
    [StringLength(25)]
    public string category { get; set; }

    [Column(TypeName = "int(11)")]
    public int level { get; set; }

    [Column(TypeName = "int(11)")]
    public int reward_pixels { get; set; }

    [Column(TypeName = "int(11)")]
    public int reward_points { get; set; }

    [Column(TypeName = "int(11)")]
    public int progress_needed { get; set; }

    [Column(TypeName = "int(11)")]
    public int game_id { get; set; }

    //1 = achievement is active and can be loaded/progressed, 0 = disabled
    public sbyte enabled { get; set; }
}
