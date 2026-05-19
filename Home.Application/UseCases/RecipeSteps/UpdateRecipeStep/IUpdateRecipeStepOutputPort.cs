namespace Home.Application.UseCases.RecipeSteps.UpdateRecipeStep;

public interface IUpdateRecipeStepOutputPort
{

    #region Methods

    Task PresentRecipeStepNoContentAsync(CancellationToken cancellationToken);
    Task PresentRecipeStepNotFoundAsync(long recipeStepID, CancellationToken cancellationToken);

    #endregion Methods

}
