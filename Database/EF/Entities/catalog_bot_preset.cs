using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class catalog_bot_preset
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Required]
    [StringLength(255)]
    public string name { get; set; }

    [Required]
    [StringLength(255)]
    public string figure { get; set; }

    [Required]
    [StringLength(255)]
    public string gender { get; set; }

    [Required]
    [StringLength(255)]
    public string motto { get; set; }

    [Required]
    [Column(TypeName = "enum('pet','generic','bartender')")]
    public string ai_type { get; set; }
}
