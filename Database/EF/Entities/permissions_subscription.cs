using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class permissions_subscription
{
    [Key]
    [Column(TypeName = "int(10)")]
    public int id { get; set; }

    [Column(TypeName = "int(10)")]
    public int subscription_id { get; set; }

    [Column(TypeName = "int(10)")]
    public int permission_id { get; set; }
}
