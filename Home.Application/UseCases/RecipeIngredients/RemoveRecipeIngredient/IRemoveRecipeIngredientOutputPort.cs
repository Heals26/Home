namespace Home.Application.UseCases.RecipeIngredients.RemoveRecipeIngredient;

public interface IRemoveRecipeIngredientOutputPort
{

    #region Methods

    Task PresentRecipeIngredientNotFoundAsync(long ingredientID, CancellationToken cancellationToken);
    Task PresentRecipeIngredientRemovedNoContentAsync(CancellationToken cancellationToken);

    #endregion Methods

}
