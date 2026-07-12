using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Keyless]
public partial class catalog_pet_race
{
    [Column(TypeName = "int(255)")]
    public int? raceid { get; set; }

    [Column(TypeName = "int(255)")]
    public int? color1 { get; set; }

    [Column(TypeName = "int(255)")]
    public int? color2 { get; set; }

    [Column(TypeName = "enum('1','0')")]
    public string has1color { get; set; }

    [Column(TypeName = "enum('1','0')")]
    public string has2color { get; set; }
}
