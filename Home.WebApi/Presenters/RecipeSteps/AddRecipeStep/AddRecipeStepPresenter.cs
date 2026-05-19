using AutoMapper;
using Home.Application.UseCases.RecipeSteps.AddRecipeStep;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.RecipeSteps.AddRecipeStep;

namespace Home.WebApi.Presenters.RecipeSteps.AddRecipeStep;

public class AddRecipeStepPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IAddRecipeStepOutputPort
{

    #region Methods

    Task IAddRecipeStepOutputPort.PresentRecipeNotFoundAsync(long recipeID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Recipe {recipeID} Not Found", cancellationToken);

    Task IAddRecipeStepOutputPort.PresentRecipeStepAddedAsync(long recipeStepID, CancellationToken cancellationToken)
        => this.CreatedAsync(recipeStepID, new AddRecipeStepApiResponse() { RecipeStepID = recipeStepID }, cancellationToken);

    #endregion Methods

}
