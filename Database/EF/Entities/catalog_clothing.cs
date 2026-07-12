using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("catalog_clothing")]
public partial class catalog_clothing
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Required]
    [StringLength(55)]
    public string clothing_name { get; set; }

    [Required]
    [StringLength(85)]
    public string clothing_parts { get; set; }
}
