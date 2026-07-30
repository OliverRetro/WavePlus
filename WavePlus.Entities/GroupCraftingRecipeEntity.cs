using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Plus.Database.EF.Entities;

[Table("group_crafting_recipes")]
public partial class GroupCraftingRecipeEntity
{
    [Key]
    [Column("id", TypeName = "int(11)")]
    public int Id { get; set; }

    [Column("group_id", TypeName = "int(11)")]
    public int GroupId { get; set; }

    [Column("recipe_id", TypeName = "int(11)")]
    public int RecipeId { get; set; }

    // secret found
    [Column("revealed", TypeName = "tinyint(1)")]
    public bool Revealed { get; set; }
}