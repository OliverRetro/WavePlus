using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class permissions_right
{
    [Key]
    [Column(TypeName = "int(10)")]
    public int id { get; set; }

    [Column(TypeName = "int(10)")]
    public int group_id { get; set; }

    [Column(TypeName = "int(10)")]
    public int permission_id { get; set; }
}
