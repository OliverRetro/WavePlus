using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

public partial class bots_pet_response
{
    [Key]
    public string pet_id { get; set; }

    [Required]
    [Column(TypeName = "text")]
    public string responses { get; set; }
}
