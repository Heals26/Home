using AutoMapper;
using Home.Application.UseCases.RecipeSteps.RemoveRecipeStep;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.RecipeSteps.RemoveRecipeStep;

public class RemoveRecipeStepPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IRemoveRecipeStepOutputPort
{

    #region Methods

    Task IRemoveRecipeStepOutputPort.PresentRecipeStepNotFoundAsync(long recipeStepID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Recipe Step {recipeStepID} Not Found", cancellationToken);

    Task IRemoveRecipeStepOutputPort.PresentRecipeStepRemovedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    #endregion Methods

}
