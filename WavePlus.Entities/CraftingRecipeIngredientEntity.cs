using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("crafting_recipe_ingredients")]
public partial class CraftingRecipeIngredientEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("recipe_id", TypeName = "int(11)")]
    public int RecipeId { get; set; }

    // 'rp_item' or 'rp_weapon'
    [Required]
    [Column("item_type")]
    public string ItemType { get; set; }

    [Column("item_id", TypeName = "int(11)")]
    public int ItemId { get; set; }

    [Column("amount", TypeName = "int(11)")]
    public int Amount { get; set; }
}