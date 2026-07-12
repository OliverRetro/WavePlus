using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("rp_crime_log")]
[Index("crime_id", Name = "idx_rp_crime_log_crime_id")]
[Index("user_id", "expired", "timestamp", Name = "idx_rp_crime_log_user_expired_time")]
public partial class rp_crime_log
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int crime_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int user_id { get; set; }

    [MaxLength(6)]
    public DateTime timestamp { get; set; }

    [Column(TypeName = "tinyint(4)")]
    public sbyte expired { get; set; }
}
