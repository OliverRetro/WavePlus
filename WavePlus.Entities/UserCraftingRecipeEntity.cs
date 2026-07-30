using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("user_crafting_recipes")]
public partial class UserCraftingRecipeEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("user_id", TypeName = "int(11)")]
    public int UserId { get; set; }

    [Column("recipe_id", TypeName = "int(11)")]
    public int RecipeId { get; set; }

    // secret found
    [Column("revealed", TypeName = "tinyint(1)")]
    public bool Revealed { get; set; }
}