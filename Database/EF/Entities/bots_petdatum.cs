using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("id", Name = "id", IsUnique = true)]
public partial class bots_petdatum
{
    [Key]
    [Column(TypeName = "int(11) unsigned")]
    public uint id { get; set; }

    [Column(TypeName = "int(11) unsigned")]
    public uint? type { get; set; }

    [StringLength(11)]
    public string race { get; set; }

    [StringLength(11)]
    public string color { get; set; }

    [Column(TypeName = "int(11)")]
    public int? energy { get; set; }

    [Column(TypeName = "int(11)")]
    public int? experience { get; set; }

    [Column(TypeName = "int(11)")]
    public int? nutrition { get; set; }

    [Column(TypeName = "int(11)")]
    public int? respect { get; set; }

    [Column(TypeName = "int(11)")]
    public int? createstamp { get; set; }

    [Column(TypeName = "int(11)")]
    public int? have_saddle { get; set; }

    [Column(TypeName = "int(11)")]
    public int? hairdye { get; set; }

    [Column(TypeName = "int(11)")]
    public int? pethair { get; set; }

    [Column(TypeName = "int(11)")]
    public int? anyone_ride { get; set; }

    [StringLength(85)]
    public string gnome_clothing { get; set; }
}
