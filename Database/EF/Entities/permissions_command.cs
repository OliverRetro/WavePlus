using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class permissions_command
{
    [Key]
    [StringLength(45)]
    public string command { get; set; }

    [Column(TypeName = "int(11)")]
    public int group_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int subscription_id { get; set; }
}
