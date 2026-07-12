using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("from_id", Name = "from_id")]
[Index("to_id", Name = "to_id")]
public partial class messenger_request
{
    [Key]
    [Column(TypeName = "int(11) unsigned")]
    public uint id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint from_id { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint to_id { get; set; }
}
