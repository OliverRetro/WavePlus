using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class group_permission
{
    [Key]
    [Column(TypeName = "int(11)")]
    public int id { get; set; }

    /// <summary>
    /// groups.id
    /// </summary>
    [Column(TypeName = "int(11)")]
    public int group_id { get; set; }

    /// <summary>
    /// groups_roles.level
    /// </summary>
    [Column(TypeName = "int(11)")]
    public int level_id { get; set; }

    /// <summary>
    /// group_permissiosn_keys.id
    /// </summary>
    [Column(TypeName = "int(11)")]
    public int permission_id { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime created_at { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? updated_at { get; set; }
}
