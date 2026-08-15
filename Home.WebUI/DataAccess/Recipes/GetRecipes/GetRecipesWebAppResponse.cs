using Home.WebUI.DataAccess.Recipes.Models;

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
    /// How involved the recipe is, matching <see cref="RecipeComplexities"/>. Null when nobody
    /// has judged it.
    /// </summary>
    public long? Complexity { get; set; }

    /// <summary>
    /// Minutes on the stove, null when unknown.
    /// </summary>
    public int? CookMinutes { get; set; }

    /// <summary>
    /// A picture of the finished dish, null when there isn't one.
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// The meals this recipe suits.
    /// </summary>
    public ICollection<RecipeMealSlotDto> MealSlots { get; set; } = [];

    /// <summary>
    /// The name of the recipe.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Minutes of chopping and measuring before cooking starts, null when unknown.
    /// </summary>
    public int? PrepMinutes { get; set; }

    /// <summary>
    /// The ID of the recipe.
    /// </summary>
    public long RecipeID { get; set; }

    /// <summary>
    /// How many the recipe feeds, null when unknown.
    /// </summary>
    public int? Servings { get; set; }

    /// <summary>
    /// An optional URL pointing to the recipe source.
    /// </summary>
    public string? Url { get; set; }

    #endregion Properties

}
