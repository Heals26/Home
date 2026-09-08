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


    /// <summary>
    /// The last day this was on the plan, up to today. Null when it has never been had.
    /// </summary>
    public DateOnly? LastHadDate { get; set; }

    public ICollection<RecipeMealSlotDto> MealSlots { get; set; }
    public string Name { get; set; }

    /// <summary>
    /// The next day this is planned for, or null.
    /// </summary>
    public DateOnly? NextPlannedDate { get; set; }

    public int? PrepMinutes { get; set; }
    public long RecipeID { get; set; }
    public int? Servings { get; set; }

    /// <summary>
    /// How many days up to today it has been on the plan.
    /// </summary>
    public int TimesHad { get; set; }

    public string Url { get; set; }

    #endregion Properties

}
