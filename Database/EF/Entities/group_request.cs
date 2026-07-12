using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Keyless]
[Index("group_id", Name = "groupid")]
[Index("user_id", Name = "userid")]
public partial class group_request
{
    [Column(TypeName = "int(11) unsigned")]
    public uint group_id { get; set; }

    [Column(TypeName = "int(11) unsigned")]
    public uint user_id { get; set; }
}
