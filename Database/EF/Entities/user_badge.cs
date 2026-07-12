using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("user_id", Name = "user_id")]
[Index("user_id", "badge_id", Name = "user_id, badge_id", IsUnique = true)]
public partial class user_badge
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint user_id { get; set; }

    [Required]
    [StringLength(100)]
    public string badge_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int badge_slot { get; set; }
}
