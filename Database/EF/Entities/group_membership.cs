using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("group_id", Name = "groupid")]
[Index("user_id", Name = "userid")]
public partial class group_membership
{
    [Key]
    [Column(TypeName = "int(11) unsigned")]
    public uint id { get; set; }

    [Column(TypeName = "int(11) unsigned")]
    public uint group_id { get; set; }

    [Column(TypeName = "int(11) unsigned")]
    public uint user_id { get; set; }

    [Column(TypeName = "smallint(6)")]
    public short level { get; set; }
}
