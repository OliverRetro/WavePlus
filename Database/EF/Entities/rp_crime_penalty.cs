using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("star_level", Name = "uniq_rp_crime_penalties_star", IsUnique = true)]
public partial class rp_crime_penalty
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int star_level { get; set; }

    [Column(TypeName = "int(11)")]
    public int fine_amount { get; set; }

    [Column(TypeName = "smallint(6)")]
    public short jail_time { get; set; }
}
