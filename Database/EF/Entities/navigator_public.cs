using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("order_num", Name = "ordernum")]
public partial class navigator_public
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int room_id { get; set; }

    [Required]
    [StringLength(64)]
    public string caption { get; set; }

    [Required]
    [StringLength(150)]
    public string description { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string image_url { get; set; }

    [Column(TypeName = "int(11)")]
    public int order_num { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string enabled { get; set; }
}
