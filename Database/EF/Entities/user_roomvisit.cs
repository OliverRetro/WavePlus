using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("entry_timestamp", Name = "entry_timestamp")]
[Index("exit_timestamp", Name = "exit_timestamp")]
[Index("user_id", Name = "user_id")]
public partial class user_roomvisit
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint user_id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint room_id { get; set; }

    public double entry_timestamp { get; set; }

    public double exit_timestamp { get; set; }

    [Column(TypeName = "int(11)")]
    public int hour { get; set; }

    [Column(TypeName = "int(11)")]
    public int minute { get; set; }
}
