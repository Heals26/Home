namespace Home.Application.UseCases.RecipeIngredients.RemoveRecipeIngredient;

public interface IRemoveRecipeIngredientOutputPort
{

    #region Methods

    Task PresentRecipeIngredientRemovedNoContentAsync(CancellationToken cancellationToken);
    Task PresentRecipeIngredientNotFoundAsync(long ingredientID, CancellationToken cancellationToken);

    #endregion Methods

}
