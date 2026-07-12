using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class client_external_badge_text
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Required]
    [StringLength(35)]
    public string badge_code { get; set; }

    [Required]
    [StringLength(75)]
    public string badge_title { get; set; }

    [Required]
    [StringLength(150)]
    public string badge_desc { get; set; }
}
