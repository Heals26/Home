using Home.Application.UseCases.Recipes.CreateRecipe;
using Home.Application.UseCases.Recipes.DeleteRecipe;
using Home.Application.UseCases.Recipes.GetRecipe;
using Home.Application.UseCases.Recipes.GetRecipes;
using Home.Application.UseCases.Recipes.UpdateRecipe;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.Recipes.CreateRecipe;
using Home.WebApi.Presenters.Recipes.DeleteRecipe;
using Home.WebApi.Presenters.Recipes.GetRecipe;
using Home.WebApi.Presenters.Recipes.GetRecipes;
using Home.WebApi.Presenters.Recipes.UpdateRecipe;
using Home.WebApi.UseCases.Recipes.CreateRecipe;
using Home.WebApi.UseCases.Recipes.GetRecipe;
using Home.WebApi.UseCases.Recipes.GetRecipes;
using Home.WebApi.UseCases.Recipes.UpdateRecipe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class RecipesController : BaseController
{

    #region Methods

    [Version1]
    [HttpPost]
    [ProducesResponseType<CreateRecipeApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRecipe(
        [FromServices] CreateRecipePresenter presenter,
        [FromBody] CreateRecipeApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new CreateRecipeInputPort(request.Name, request.Url), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpDelete("{recipeID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteRecipe(
        [FromServices] DeleteRecipePresenter presenter,
        [FromRoute] long recipeID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteRecipeInputPort(recipeID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpGet("{recipeID}")]
    [ProducesResponseType<GetRecipeApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecipe(
        [FromServices] GetRecipePresenter presenter,
        [FromRoute] long recipeID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetRecipeInputPort(recipeID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpGet]
    [ProducesResponseType<GetRecipesApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecipes(
        [FromServices] GetRecipesPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetRecipesInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [Version1]
    [HttpPatch("{recipeID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateRecipe(
        [FromServices] UpdateRecipePresenter presenter,
        [FromRoute] long recipeID,
        [FromBody] UpdateRecipeApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new UpdateRecipeInputPort(recipeID, request.Name, request.Url), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
