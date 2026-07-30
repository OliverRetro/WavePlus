using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("StarLevel", Name = "uniq_rp_crime_penalties_star", IsUnique = true)]
[Table("rp_crime_penalties")]
public partial class RpCrimePenaltyEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("star_level", TypeName = "int(11)")]
    public int StarLevel { get; set; }

    [Column("fine_amount", TypeName = "int(11)")]
    public int FineAmount { get; set; }

    [Column("jail_time", TypeName = "smallint(6)")]
    public short JailTime { get; set; }
}