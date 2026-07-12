using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class catalog_promotion
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [StringLength(35)]
    public string title { get; set; }

    [StringLength(75)]
    public string image { get; set; }

    [Column(TypeName = "int(11)")]
    public int? unknown { get; set; }

    [StringLength(35)]
    public string page_link { get; set; }

    [Column(TypeName = "int(11)")]
    public int? parent_id { get; set; }
}
