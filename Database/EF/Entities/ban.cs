using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("bantype", Name = "bantype")]
[Index("value", Name = "value")]
public partial class ban
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Required]
    [Column(TypeName = "enum('user','ip','machine')")]
    public string bantype { get; set; }

    [Required]
    [StringLength(50)]
    public string value { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string reason { get; set; }

    public double expire { get; set; }

    [Required]
    [StringLength(50)]
    public string added_by { get; set; }

    [Required]
    [StringLength(50)]
    public string added_date { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1','2')")]
    public string appeal_state { get; set; }
}
