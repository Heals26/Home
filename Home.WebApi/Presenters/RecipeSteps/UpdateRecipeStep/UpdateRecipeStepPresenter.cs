using AutoMapper;
using Home.Application.UseCases.RecipeSteps.UpdateRecipeStep;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.RecipeSteps.UpdateRecipeStep;

public class UpdateRecipeStepPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IUpdateRecipeStepOutputPort
{

    #region Methods

    Task IUpdateRecipeStepOutputPort.PresentRecipeStepNoContentAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IUpdateRecipeStepOutputPort.PresentRecipeStepNotFoundAsync(long recipeStepID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Recipe Step {recipeStepID} Not Found", cancellationToken);

    #endregion Methods

}
