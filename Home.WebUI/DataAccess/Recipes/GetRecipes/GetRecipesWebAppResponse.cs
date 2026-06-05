namespace Home.WebUI.DataAccess.Recipes.GetRecipes;

public class GetRecipesWebAppResponse
{

    #region Properties

    /// <summary>
    /// A collection of recipes.
    /// </summary>
    public ICollection<GetRecipeDto> Recipes { get; set; } = [];

    #endregion Properties

}

public class GetRecipeDto
{

    #region Properties

    /// <summary>
    /// The name of the recipe.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the recipe.
    /// </summary>
    public long RecipeID { get; set; }

    /// <summary>
    /// An optional URL pointing to the recipe source.
    /// </summary>
    public string? Url { get; set; }

    #endregion Properties

}
