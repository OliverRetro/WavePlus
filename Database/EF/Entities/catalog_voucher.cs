using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class catalog_voucher
{
    [Key]
    [StringLength(45)]
    public string voucher { get; set; }

    [Required]
    [Column(TypeName = "enum('credits','duckets')")]
    public string type { get; set; }

    [Column(TypeName = "int(11)")]
    public int value { get; set; }

    [Column(TypeName = "int(11)")]
    public int current_uses { get; set; }

    [Column(TypeName = "int(11)")]
    public int max_uses { get; set; }

    [Column(TypeName = "enum('0','1')")]
    public string enabled { get; set; }
}
