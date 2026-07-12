using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class quest
{
    [Key]
    [Column(TypeName = "int(10) unsigned")]
    public uint id { get; set; }

    [Required]
    [StringLength(32)]
    public string type { get; set; }

    [Column(TypeName = "int(11)")]
    public int level_num { get; set; }

    [Column(TypeName = "int(10)")]
    public int goal_type { get; set; }

    [Column(TypeName = "int(10) unsigned")]
    public uint goal_data { get; set; }

    [Required]
    [StringLength(32)]
    public string action { get; set; }

    [Column(TypeName = "int(11)")]
    public int pixel_reward { get; set; }

    [Required]
    [StringLength(2)]
    public string data_bit { get; set; }

    [Required]
    [Column(TypeName = "enum('0','1','2','3','4','5')")]
    public string reward_type { get; set; }

    [Column(TypeName = "int(11)")]
    public int timestamp_unlock { get; set; }

    [Column(TypeName = "int(11)")]
    public int timestamp_lock { get; set; }
}
