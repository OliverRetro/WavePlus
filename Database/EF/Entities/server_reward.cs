using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class server_reward
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    [Column(TypeName = "int(11)")]
    public int reward_start { get; set; }

    [Column(TypeName = "int(11)")]
    public int reward_end { get; set; }

    [Required]
    [Column(TypeName = "enum('credits','badge','diamonds','duckets','none')")]
    public string reward_type { get; set; }

    [Required]
    [StringLength(255)]
    public string reward_data { get; set; }

    [Required]
    [StringLength(255)]
    public string message { get; set; }

    [Required]
    [Column(TypeName = "enum('1','0')")]
    public string enabled { get; set; }
}
