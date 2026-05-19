namespace Home.Application.UseCases.RecipeIngredients.UpdateRecipeIngredient;

public interface IUpdateRecipeIngredientOutputPort
{

    #region Methods

    Task PresentRecipeIngredientNoContentAsync(CancellationToken cancellationToken);
    Task PresentRecipeIngredientNotFoundAsync(long ingredientID, CancellationToken cancellationToken);

    #endregion Methods

}
