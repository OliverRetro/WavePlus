using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class rp_skills_level
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int skill_id { get; set; }

    [Required]
    [StringLength(25)]
    public string level { get; set; }

    [Column(TypeName = "int(11)")]
    public int required_progress { get; set; }
}
