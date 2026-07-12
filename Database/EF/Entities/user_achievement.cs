using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[PrimaryKey("userid", "group")]
[Index("userid", Name = "id")]
public partial class user_achievement
{
    [Key]
    [Column(TypeName = "int(11) unsigned")]
    public uint userid { get; set; }

    [Key]
    public string group { get; set; }

    [Column(TypeName = "int(11)")]
    public int level { get; set; }

    [Column(TypeName = "int(11)")]
    public int progress { get; set; }
}
