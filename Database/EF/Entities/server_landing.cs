using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("server_landing")]
[Index("id", Name = "id", IsUnique = true)]
public partial class server_landing
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [StringLength(35)]
    public string title { get; set; }

    [Column(TypeName = "text")]
    public string text { get; set; }

    [StringLength(25)]
    public string button_text { get; set; }

    [Column(TypeName = "enum('0','1','2','3')")]
    public string button_type { get; set; }

    [StringLength(90)]
    public string button_link { get; set; }

    [StringLength(120)]
    public string image_link { get; set; }
}
