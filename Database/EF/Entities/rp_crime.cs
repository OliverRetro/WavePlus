using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("crime_tag", Name = "uniq_rp_crimes_tag", IsUnique = true)]
public partial class rp_crime
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Required]
    [StringLength(32)]
    public string crime_tag { get; set; }

    [Required]
    [StringLength(55)]
    public string crime_name { get; set; }

    [Column(TypeName = "smallint(6)")]
    public short severity { get; set; }

    [Column(TypeName = "smallint(6)")]
    public short time_active { get; set; }

    [Column(TypeName = "tinyint(4)")]
    public sbyte automatic_report { get; set; }

    [Column(TypeName = "tinyint(4)")]
    public sbyte police_alert { get; set; }

    [Required]
    [StringLength(255)]
    public string note { get; set; }
}
