using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("user_id", Name = "user_id")]
[Index("search_code", Name = "value")]
public partial class user_saved_search
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int user_id { get; set; }

    [Required]
    [StringLength(65)]
    public string filter { get; set; }

    [Required]
    [StringLength(65)]
    public string search_code { get; set; }
}
