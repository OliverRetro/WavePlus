using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("tele_one_id", Name = "tele_one_id")]
public partial class room_items_tele_link
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint tele_one_id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint tele_two_id { get; set; }
}
