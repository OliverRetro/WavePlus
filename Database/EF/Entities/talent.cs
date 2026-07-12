using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class talent
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Required]
    [Column(TypeName = "enum('citizenship','helper')")]
    public string type { get; set; }

    [Column(TypeName = "int(11)")]
    public int? level { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string data_actions { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string data_gifts { get; set; }
}
