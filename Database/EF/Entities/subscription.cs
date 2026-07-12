using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class subscription
{
    [Key]
    [Column(TypeName = "int(10)")]
    public int id { get; set; }

    [Required]
    [StringLength(50)]
    public string name { get; set; }

    [Required]
    [StringLength(10)]
    public string badge_code { get; set; }

    [Column(TypeName = "int(11)")]
    public int credits { get; set; }

    [Column(TypeName = "int(11)")]
    public int duckets { get; set; }

    [Column(TypeName = "int(11)")]
    public int respects { get; set; }

    [Column(TypeName = "smallint(6)")]
    public short static_aggression { get; set; }

    [Column(TypeName = "smallint(6)")]
    public short trash_search_cooldown { get; set; }
}
