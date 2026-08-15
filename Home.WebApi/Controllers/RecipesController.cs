using Home.Application.UseCases.Recipes.CreateRecipe;
using Home.Application.UseCases.Recipes.DeleteRecipe;
using Home.Application.UseCases.Recipes.GetRecipe;
using Home.Application.UseCases.Recipes.GetRecipes;
using Home.Application.UseCases.Recipes.ImportRecipe;
using Home.Application.UseCases.Recipes.SetRecipeMealSlots;
using Home.Application.UseCases.Recipes.UpdateRecipe;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.Recipes.CreateRecipe;
using Home.WebApi.Presenters.Recipes.DeleteRecipe;
using Home.WebApi.Presenters.Recipes.GetRecipe;
using Home.WebApi.Presenters.Recipes.GetRecipes;
using Home.WebApi.Presenters.Recipes.ImportRecipe;
using Home.WebApi.Presenters.Recipes.SetRecipeMealSlots;
using Home.WebApi.Presenters.Recipes.UpdateRecipe;
using Home.WebApi.UseCases.Recipes.CreateRecipe;
using Home.WebApi.UseCases.Recipes.GetRecipe;
using Home.WebApi.UseCases.Recipes.GetRecipes;
using Home.WebApi.UseCases.Recipes.ImportRecipe;
using Home.WebApi.UseCases.Recipes.SetRecipeMealSlots;
using Home.WebApi.UseCases.Recipes.UpdateRecipe;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class RecipesController : BaseController
{

    #region Methods

    [HttpPost]
    [ProducesResponseType<CreateRecipeApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRecipe(
        [FromServices] CreateRecipePresenter presenter,
        [FromBody] CreateRecipeApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(
            new CreateRecipeInputPort(request.Complexity, request.CookMinutes, request.ImageUrl, request.Name, request.PrepMinutes, request.Servings, request.Url),
            presenter,
            this.ServiceFactory,
            cancellationToken);

        return presenter.Result;
    }

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

    [HttpGet]
    [ProducesResponseType<GetRecipesApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecipes(
        [FromServices] GetRecipesPresenter presenter,
        [FromQuery] long? mealSlotID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetRecipesInputPort(mealSlotID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPost("Import")]
    [ProducesResponseType<ImportRecipeApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> ImportRecipe(
        [FromServices] ImportRecipePresenter presenter,
        [FromBody] ImportRecipeApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new ImportRecipeInputPort(request.Url), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPut("{recipeID}/MealSlots")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetRecipeMealSlots(
        [FromServices] SetRecipeMealSlotsPresenter presenter,
        [FromRoute] long recipeID,
        [FromBody] SetRecipeMealSlotsApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new SetRecipeMealSlotsInputPort(request.MealSlotIDs ?? [], recipeID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{recipeID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateRecipe(
        [FromServices] UpdateRecipePresenter presenter,
        [FromRoute] long recipeID,
        [FromBody] UpdateRecipeApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(
            new UpdateRecipeInputPort(request.Complexity, request.CookMinutes, request.ImageUrl, request.Name, request.PrepMinutes, recipeID, request.Servings, request.Url),
            presenter,
            this.ServiceFactory,
            cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
