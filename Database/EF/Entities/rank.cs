using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("id", Name = "id", IsUnique = true)]
public partial class rank
{
    [Key]
    [Column(TypeName = "int(11) unsigned")]
    public uint id { get; set; }

    [Required]
    [StringLength(50)]
    public string name { get; set; }

    [Required]
    [StringLength(5)]
    public string badgeid { get; set; }

    [Required]
    [StringLength(50)]
    public string title { get; set; }

    [Required]
    [Column(TypeName = "enum('red','green','pixeldarkblue','orange','blue','settings','pixellightblue')")]
    public string tab_colour { get; set; }
}
