namespace Home.WebApi.UseCases.Recipes.CreateRecipe;

public record CreateRecipeApiRequest(
    long? Complexity,
    int? CookMinutes,
    string ImageUrl,
    string Name,
    int? PrepMinutes,
    int? Servings,
    string Url);
