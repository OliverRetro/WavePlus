using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[PrimaryKey("id", "type")]
public partial class groups_item
{
    [Key]
    [Column(TypeName = "enum('base','symbol','color','color2','color3')")]
    public string type { get; set; }

    [Key]
    [Column(TypeName = "int(255)")]
    public int id { get; set; }

    [Required]
    [StringLength(255)]
    public string firstvalue { get; set; }

    [Required]
    [StringLength(2000)]
    public string secondvalue { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string enabled { get; set; }
}
