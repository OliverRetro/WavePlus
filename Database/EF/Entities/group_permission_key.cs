using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class group_permission_key
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Required]
    [StringLength(55)]
    public string name { get; set; }

    [Required]
    [StringLength(55)]
    public string key { get; set; }

    [Required]
    [StringLength(255)]
    public string description { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string type_specific { get; set; }
}
