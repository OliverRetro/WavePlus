using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("group_id", Name = "group_id")]
[Index("id", Name = "id", IsUnique = true)]
public partial class items_group
{
    [Key]
    [Column(TypeName = "int(11) unsigned")]
    public uint id { get; set; }

    [Column(TypeName = "int(11)")]
    public int group_id { get; set; }
}
