using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class navigator_category
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Required]
    [Column(TypeName = "enum('official_view','hotel_view','myworld_view','roomads_view','query')")]
    public string category { get; set; }

    [Required]
    [StringLength(35)]
    public string category_identifier { get; set; }

    [Required]
    [StringLength(35)]
    public string public_name { get; set; }

    [Required]
    [Column(TypeName = "enum('REGULAR','THUMBNAIL')")]
    public string view_mode { get; set; }

    [Column(TypeName = "int(11)")]
    public int required_rank { get; set; }

    [Required]
    [StringLength(25)]
    public string category_type { get; set; }

    [Required]
    [Column(TypeName = "enum('NOTHING','SHOW_MORE')")]
    public string search_allowance { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1')")]
    public string enabled { get; set; }

    [Column(TypeName = "int(11)")]
    public int order_id { get; set; }
}
