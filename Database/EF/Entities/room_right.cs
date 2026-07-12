using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("room_id", Name = "room_id")]
[Index("user_id", Name = "user_id")]
public partial class room_right
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint room_id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint user_id { get; set; }
}
