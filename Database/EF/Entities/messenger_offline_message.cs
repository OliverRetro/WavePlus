using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class messenger_offline_message
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11) unsigned")]
    public uint to_id { get; set; }

    [Column(TypeName = "int(11) unsigned")]
    public uint from_id { get; set; }

    [Required]
    [StringLength(255)]
    public string message { get; set; }

    public double timestamp { get; set; }
}
