using Home.Application.Infrastructure.Recipes;
using Home.Application.UseCases.RecipeImages.DeleteRecipeImage;
using Home.Application.UseCases.RecipeImages.GetRecipeImage;
using Home.Application.UseCases.RecipeImages.SetRecipeImage;
using Home.Application.UseCases.Recipes.CreateRecipe;
using Home.Application.UseCases.Recipes.DeleteRecipe;
using Home.Application.UseCases.Recipes.GetRecipe;
using Home.Application.UseCases.Recipes.GetRecipes;
using Home.Application.UseCases.Recipes.ImportRecipe;
using Home.Application.UseCases.Recipes.SetRecipeMealSlots;
using Home.Application.UseCases.Recipes.UpdateRecipe;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.RecipeImages.DeleteRecipeImage;
using Home.WebApi.Presenters.RecipeImages.GetRecipeImage;
using Home.WebApi.Presenters.RecipeImages.SetRecipeImage;
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

    [HttpDelete("{recipeID}/Image")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteRecipeImage(
        [FromServices] DeleteRecipeImagePresenter presenter,
        [FromRoute] long recipeID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteRecipeImageInputPort(recipeID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet("{recipeID}/Image")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecipeImage(
        [FromServices] GetRecipeImagePresenter presenter,
        [FromRoute] long recipeID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetRecipeImageInputPort(recipeID), presenter, this.ServiceFactory, cancellationToken);

        // The image's URL carries its UpdatedOnUTC ticks, so a changed photo is a changed URL and
        // this can be cached as hard as the browser likes.
        if (presenter.PresentedSuccessfully)
            this.Response.Headers.CacheControl = "private, max-age=31536000, immutable";

        return presenter.Result;
    }

    /// <summary>
    /// The photo arrives as multipart form data — the only shape a browser file input produces.
    /// </summary>
    [HttpPut("{recipeID}/Image")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [RequestSizeLimit(RecipeImageLogic.MaximumContentBytes + 65536)]
    public async Task<IActionResult> SetRecipeImage(
        [FromServices] SetRecipeImagePresenter presenter,
        [FromRoute] long recipeID,
        IFormFile image,
        CancellationToken cancellationToken)
    {
        using var _Content = new MemoryStream();

        if (image != null)
            await image.CopyToAsync(_Content, cancellationToken);

        await this.Pipeline.InvokeAsync(
            new SetRecipeImageInputPort(_Content.ToArray(), recipeID),
            presenter,
            this.ServiceFactory,
            cancellationToken);

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
