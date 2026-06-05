namespace Home.WebUI.DataAccess.Recipes.CreateRecipe;

public class CreateRecipeWebAppRequest
{

    #region Properties

    /// <summary>
    /// The name of the recipe.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// An optional URL pointing to the recipe source.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    #endregion Properties

}
