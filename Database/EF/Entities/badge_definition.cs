using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("code", Name = "code", IsUnique = true)]
public partial class badge_definition
{
    [Key]
    [StringLength(35)]
    public string code { get; set; }

    [Required]
    [StringLength(25)]
    public string required_right { get; set; }
}
