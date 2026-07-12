using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("user_id", "equipped", Name = "idx_user_rp_items_equipped")]
[Index("user_id", "item_id", Name = "idx_user_rp_items_user_item")]
public partial class user_rp_item
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int item_id { get; set; }

    [Column(TypeName = "int(11)")]
    public int user_id { get; set; }

    [Column(TypeName = "smallint(6)")]
    public short uses { get; set; }

    [Column(TypeName = "int(11)")]
    public int slot { get; set; }

    [Column(TypeName = "int(11)")]
    public int durability_left { get; set; }

    public bool equipped { get; set; }
}
