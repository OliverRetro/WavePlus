using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class catalog_marketplace_datum
{
    [Key]
    [Column(TypeName = "int(12)")]
    public int id { get; set; }

    [Column(TypeName = "int(7)")]
    public int sprite { get; set; }

    [Column(TypeName = "int(7)")]
    public int sold { get; set; }

    [Column(TypeName = "int(9)")]
    public int avgprice { get; set; }
}
