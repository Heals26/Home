namespace Home.Application.UseCases.RecipeSteps.AddRecipeStep;

public interface IAddRecipeStepOutputPort
{

    #region Methods

    Task PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken);
    Task PresentRecipeStepAddedAsync(long recipeStepID, CancellationToken cancellationToken);

    #endregion Methods

}
