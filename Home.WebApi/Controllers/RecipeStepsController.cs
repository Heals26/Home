using Home.Application.UseCases.RecipeSteps.AddRecipeStep;
using Home.Application.UseCases.RecipeSteps.RemoveRecipeStep;
using Home.Application.UseCases.RecipeSteps.UpdateRecipeStep;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.RecipeSteps.AddRecipeStep;
using Home.WebApi.Presenters.RecipeSteps.RemoveRecipeStep;
using Home.WebApi.Presenters.RecipeSteps.UpdateRecipeStep;
using Home.WebApi.UseCases.RecipeSteps.AddRecipeStep;
using Home.WebApi.UseCases.RecipeSteps.UpdateRecipeStep;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class RecipeStepsController : BaseController
{

    #region Methods

    [HttpPost]
    [ProducesResponseType<AddRecipeStepApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddRecipeStep(
        [FromServices] AddRecipeStepPresenter presenter,
        [FromBody] AddRecipeStepApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(
            new AddRecipeStepInputPort(request.Content, request.RecipeID, request.Sequence, request.Title),
            presenter,
            this.ServiceFactory,
            cancellationToken);

        return presenter.Result;
    }

    [HttpDelete("{recipeStepID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveRecipeStep(
        [FromServices] RemoveRecipeStepPresenter presenter,
        [FromRoute] long recipeStepID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new RemoveRecipeStepInputPort(recipeStepID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{recipeStepID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateRecipeStep(
        [FromServices] UpdateRecipeStepPresenter presenter,
        [FromRoute] long recipeStepID,
        [FromBody] UpdateRecipeStepApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(
            new UpdateRecipeStepInputPort(recipeStepID, request.Content, request.Sequence, request.Title),
            presenter,
            this.ServiceFactory,
            cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
