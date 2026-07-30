using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Plus.Database.EF.Entities;

[Index("UserId", Name = "user_id")]
[Index("SearchCode", Name = "value")]
[Table("user_saved_searches")]
public partial class UserSavedSearchEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Required]
    [StringLength(65)]
    [Column("filter")]
    public string Filter { get; set; }

    [Required]
    [StringLength(65)]
    [Column("search_code")]
    public string SearchCode { get; set; }
}