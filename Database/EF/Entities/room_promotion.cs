using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class room_promotion
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int room_id { get; set; }

    [Required]
    [StringLength(35)]
    public string title { get; set; }

    [Required]
    [StringLength(220)]
    public string description { get; set; }

    public double timestamp_start { get; set; }

    public double timestamp_expire { get; set; }

    [Column(TypeName = "int(11)")]
    public int category_id { get; set; }
}
