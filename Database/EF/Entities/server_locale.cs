using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("server_locale")]
public partial class server_locale
{
    [Key]
    public string key { get; set; }

    [Column(TypeName = "text")]
    public string value { get; set; }
}
