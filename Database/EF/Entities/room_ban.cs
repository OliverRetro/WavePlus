using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[PrimaryKey("user_id", "room_id")]
[Index("room_id", Name = "room_id")]
[Index("user_id", Name = "user_id")]
public partial class room_ban
{
    [Key]
    [Column(TypeName = "int(11) unsigned")]
    public uint user_id { get; set; }

    [Key]
    [Column(TypeName = "int(11) unsigned")]
    public uint room_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int expire { get; set; }
}
