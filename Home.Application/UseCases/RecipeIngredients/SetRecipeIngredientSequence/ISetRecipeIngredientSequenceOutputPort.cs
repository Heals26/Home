namespace Home.Application.UseCases.RecipeIngredients.SetRecipeIngredientSequence;

public interface ISetRecipeIngredientSequenceOutputPort
{

    #region Methods

    Task PresentRecipeIngredientNotFoundAsync(long ingredientID, CancellationToken cancellationToken);
    Task PresentRecipeIngredientSequenceSetAsync(CancellationToken cancellationToken);

    #endregion Methods

}
