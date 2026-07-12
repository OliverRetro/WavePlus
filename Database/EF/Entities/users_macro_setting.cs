using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class users_macro_setting
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int user_id { get; set; }

    public bool enabled { get; set; }
}
