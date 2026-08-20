using Home.WebApi.UseCases.Recipes.Models;

namespace Home.WebApi.UseCases.Recipes.GetRecipes;

public class GetRecipesApiResponse
{

    #region Properties

    public ICollection<GetRecipeDto> Recipes { get; set; }

    #endregion Properties

}

public class GetRecipeDto
{

    #region Properties

    public long? Complexity { get; set; }
    public int? CookMinutes { get; set; }
    public string ImageUrl { get; set; }

    /// <summary>
    /// Ticks of the household photo's last upload — null when there is no photo. Doubles as the
    /// cache-buster in the image's URL.
    /// </summary>
    public long? ImageVersion { get; set; }

    public ICollection<RecipeMealSlotDto> MealSlots { get; set; }
    public string Name { get; set; }
    public int? PrepMinutes { get; set; }
    public long RecipeID { get; set; }
    public int? Servings { get; set; }
    public string Url { get; set; }

    #endregion Properties

}
