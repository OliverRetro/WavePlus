using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class server_setting
{
    [Key]
    public string key { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string value { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string description { get; set; }
}
