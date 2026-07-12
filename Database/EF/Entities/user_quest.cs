using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[PrimaryKey("user_id", "quest_id")]
public partial class user_quest
{
    [Key]
    [Column(TypeName = "int(10) unsigned")]
    public uint user_id { get; set; }

    [Key]
    [Column(TypeName = "int(10) unsigned")]
    public uint quest_id { get; set; }

    [Column(TypeName = "int(10)")]
    public int? progress { get; set; }
}
