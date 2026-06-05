using Home.WebUI.DataAccess.Recipes.Models;

namespace Home.WebUI.DataAccess.Recipes.GetRecipe;

public class GetRecipeWebAppResponse
{

    #region Properties

    /// <summary>
    /// The ingredients that make up the recipe.
    /// </summary>
    public List<RecipeIngredientDto> Ingredients { get; set; } = [];

    /// <summary>
    /// The name of the recipe.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The notes attached to the recipe.
    /// </summary>
    public List<RecipeNoteDto> Notes { get; set; } = [];

    /// <summary>
    /// The ID of the recipe.
    /// </summary>
    public long RecipeID { get; set; }

    /// <summary>
    /// The ordered preparation steps for the recipe.
    /// </summary>
    public List<RecipeStepDto> Steps { get; set; } = [];

    /// <summary>
    /// An optional URL pointing to the recipe source.
    /// </summary>
    public string? Url { get; set; }

    #endregion Properties

}
