namespace Home.WebUI.DataAccess.Recipes.CreateRecipe;

public class CreateRecipeWebAppRequest
{

    #region Properties

    /// <summary>
    /// How involved the recipe is. Null when nobody has judged it.
    /// </summary>
    public long? Complexity { get; set; }

    /// <summary>
    /// Minutes on the stove, null when unknown.
    /// </summary>
    public int? CookMinutes { get; set; }

    /// <summary>
    /// A picture of the finished dish.
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// The name of the recipe.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Minutes of chopping and measuring before cooking starts, null when unknown.
    /// </summary>
    public int? PrepMinutes { get; set; }

    /// <summary>
    /// How many the recipe feeds, null when unknown.
    /// </summary>
    public int? Servings { get; set; }

    /// <summary>
    /// An optional URL pointing to the recipe source.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    #endregion Properties

}
