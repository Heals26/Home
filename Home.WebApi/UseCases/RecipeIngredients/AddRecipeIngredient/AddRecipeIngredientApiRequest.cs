namespace Home.WebApi.UseCases.RecipeIngredients.AddRecipeIngredient;

public record AddRecipeIngredientApiRequest(
    string Name,
    decimal? Quantity,
    long RecipeID,
    decimal? Volume,
    decimal? Weight);
