using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("ignore_id", Name = "ignore_id")]
[Index("user_id", Name = "user_id")]
public partial class user_ignore
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint user_id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint ignore_id { get; set; }
}
