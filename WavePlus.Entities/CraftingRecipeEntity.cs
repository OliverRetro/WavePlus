using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("crafting_recipes")]
public partial class CraftingRecipeEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Required]
    [StringLength(64)]
    [Column("name")]
    public string Name { get; set; }

    // 'rp_item' or 'rp_weapon'
    [Required]
    [Column("reward_type")]
    public string RewardType { get; set; }

    [Column("reward_id", TypeName = "int(11)")]
    public int RewardId { get; set; }

    [Column("reward_amount", TypeName = "int(11)")]
    public int RewardAmount { get; set; }

    [Column("secret", TypeName = "tinyint(1)")]
    public bool Secret { get; set; }

    [Column("enabled", TypeName = "tinyint(1)")]
    public bool Enabled { get; set; }
}