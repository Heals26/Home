using Home.Domain.Entities;

namespace Home.Application.UseCases.RecipeSteps.GetRecipeStep;

public interface IGetRecipeStepOutputPort
{

    #region Methods

    Task PresentRecipeStepAsync(RecipeStep recipeStep, CancellationToken cancellationToken);
    Task PresentRecipeStepNotFoundAsync(long recipeStepID, CancellationToken cancellationToken);

    #endregion Methods

}
