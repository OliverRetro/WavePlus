using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Keyless]
[Table("server_status")]
[Index("users_online", Name = "users_online", IsUnique = true)]
public partial class server_status
{
    [Column(TypeName = "int(11)")]
    public int users_online { get; set; }

    [Column(TypeName = "int(11)")]
    public int loaded_rooms { get; set; }
}
