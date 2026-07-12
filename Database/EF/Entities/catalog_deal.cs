using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class catalog_deal
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string items { get; set; }

    [Required]
    [StringLength(35)]
    public string name { get; set; }

    [Column(TypeName = "int(11)")]
    public int room_id { get; set; }
}
