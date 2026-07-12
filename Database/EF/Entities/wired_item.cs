using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("id", Name = "id", IsUnique = true)]
public partial class wired_item
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Required]
    [StringLength(5000)]
    public string items { get; set; }

    [Column(TypeName = "int(11)")]
    public int delay { get; set; }

    [Required]
    [Column("string")]
    [StringLength(5000)]
    public string _string { get; set; }

    [Required]
    [Column("bool", TypeName = "enum('0','1')")]
    public string _bool { get; set; }
}
