namespace Home.WebApi.UseCases.RecipeIngredients.AddRecipeIngredient;

public record AddRecipeIngredientApiRequest(
    decimal? Amount,
    string Name,
    long RecipeID,
    long? Unit);
