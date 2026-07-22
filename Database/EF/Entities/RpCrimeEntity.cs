using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("CrimeTag", Name = "uniq_rp_crimes_tag", IsUnique = true)]
[Table("rp_crimes")]
public partial class RpCrimeEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [StringLength(32)]
    [Column("crime_tag")]
    public string CrimeTag { get; set; }

    [Required]
    [StringLength(55)]
    [Column("crime_name")]
    public string CrimeName { get; set; }

    [Column("severity", TypeName = "smallint(6)")]
    public short Severity { get; set; }

    [Column("time_active", TypeName = "smallint(6)")]
    public short TimeActive { get; set; }

    [Column("automatic_report", TypeName = "tinyint(4)")]
    public sbyte AutomaticReport { get; set; }

    [Column("police_alert", TypeName = "tinyint(4)")]
    public sbyte PoliceAlert { get; set; }

    [Required]
    [StringLength(255)]
    [Column("note")]
    public string Note { get; set; }
}