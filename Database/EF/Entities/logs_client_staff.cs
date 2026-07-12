using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Table("logs_client_staff")]
public partial class logs_client_staff
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int user_id { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string data_string { get; set; }

    [Required]
    [StringLength(75)]
    public string machine_id { get; set; }

    public double timestamp { get; set; }
}
