using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class talents_sub_level
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int talent_level { get; set; }

    [Column(TypeName = "int(11)")]
    public int sub_level { get; set; }

    [Required]
    [StringLength(45)]
    public string badge_code { get; set; }

    [Column(TypeName = "int(11)")]
    public int required_progress { get; set; }
}
