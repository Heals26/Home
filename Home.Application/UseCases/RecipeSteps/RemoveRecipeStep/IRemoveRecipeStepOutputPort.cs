namespace Home.Application.UseCases.RecipeSteps.RemoveRecipeStep;

public interface IRemoveRecipeStepOutputPort
{

    #region Methods

    Task PresentRecipeStepNotFoundAsync(long recipeStepID, CancellationToken cancellationToken);
    Task PresentRecipeStepRemovedAsync(CancellationToken cancellationToken);

    #endregion Methods

}
