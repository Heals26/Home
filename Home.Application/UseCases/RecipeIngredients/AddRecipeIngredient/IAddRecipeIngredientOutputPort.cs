namespace Home.Application.UseCases.RecipeIngredients.AddRecipeIngredient;

public interface IAddRecipeIngredientOutputPort
{

    #region Methods

    Task PresentRecipeIngredientAddedAsync(long ingredientID, CancellationToken cancellationToken);
    Task PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken);

    #endregion Methods

}
