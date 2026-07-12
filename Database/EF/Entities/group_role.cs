using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class group_role
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    /// <summary>
    /// Group&apos;s ID.
    /// </summary>
    [Column(TypeName = "int(11)")]
    public int group_id { get; set; }

    /// <summary>
    /// Levels go in order of Slave -&gt; CEO. 1 = Slave, 100 = CEO.
    /// </summary>
    [Column(TypeName = "int(11)")]
    public int level { get; set; }

    /// <summary>
    /// Literal name displayed; ex. general manager
    /// </summary>
    [Required]
    [StringLength(55)]
    public string name { get; set; }

    /// <summary>
    /// Credits paid via intervals.
    /// </summary>
    [Column(TypeName = "int(11)")]
    public int shift_pay { get; set; }

    /// <summary>
    /// Shift-Costume (full) to replace users own clothes. Blank for no swap.
    /// </summary>
    [Required]
    [StringLength(255)]
    public string shift_costume { get; set; }

    /// <summary>
    /// The duration at which worker will be auto-rewarded. In minutes.
    /// </summary>
    [Column(TypeName = "smallint(6)")]
    public short? shift_duration { get; set; }

    /// <summary>
    /// Replace users motto for the duration of shift? Blank for no swap.
    /// </summary>
    [Required]
    [StringLength(40)]
    public string shift_motto { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime created_at { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? updated_at { get; set; }
}
