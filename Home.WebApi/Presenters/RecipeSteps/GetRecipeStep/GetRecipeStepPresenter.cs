using AutoMapper;
using Home.Application.UseCases.RecipeSteps.GetRecipeStep;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.RecipeSteps.GetRecipeStep;

namespace Home.WebApi.Presenters.RecipeSteps.GetRecipeStep;

public class GetRecipeStepPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetRecipeStepOutputPort
{

    #region Methods

    Task IGetRecipeStepOutputPort.PresentRecipeStepAsync(RecipeStep recipeStep, CancellationToken cancellationToken)
        => this.OkAsync(new GetRecipeStepApiResponse()
        {
            RecipeStepID = recipeStep.RecipeStepID,
            Content = recipeStep.Content,
            Sequence = recipeStep.Sequence,
            Title = recipeStep.Title
        }, cancellationToken);

    Task IGetRecipeStepOutputPort.PresentRecipeStepNotFoundAsync(long recipeStepID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Recipe Step {recipeStepID} Not Found", cancellationToken);

    #endregion Methods

}
