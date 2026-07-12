using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("logs_client_namechange")]
public partial class logs_client_namechange
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int user_id { get; set; }

    [Required]
    [StringLength(50)]
    public string new_name { get; set; }

    [Required]
    [StringLength(50)]
    public string old_name { get; set; }

    [Column(TypeName = "int(11)")]
    public int timestamp { get; set; }
}
