using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("user_one_id", Name = "user_one_id")]
[Index("user_two_id", Name = "user_two_id")]
public partial class messenger_friendship
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint user_one_id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint user_two_id { get; set; }
}
