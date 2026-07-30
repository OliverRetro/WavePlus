using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("rp_crime_log")]
[Index("CrimeId", Name = "idx_rp_crime_log_crime_id")]
[Index("UserId", "Expired", "Timestamp", Name = "idx_rp_crime_log_user_expired_time")]
public partial class RpCrimeLogEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("crime_id", TypeName = "int(11)")]
    public int CrimeId { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Column("timestamp", TypeName = "int(11)")]
    public int Timestamp { get; set; }

    [Column("expired", TypeName = "tinyint(4)")]
    public sbyte Expired { get; set; }
}