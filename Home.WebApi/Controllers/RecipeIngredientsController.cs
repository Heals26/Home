using Home.Application.UseCases.RecipeIngredients.AddRecipeIngredient;
using Home.Application.UseCases.RecipeIngredients.GetIngredientSuggestions;
using Home.Application.UseCases.RecipeIngredients.GetRecipeIngredient;
using Home.Application.UseCases.RecipeIngredients.RemoveRecipeIngredient;
using Home.Application.UseCases.RecipeIngredients.SetRecipeIngredientSequence;
using Home.Application.UseCases.RecipeIngredients.UpdateRecipeIngredient;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.RecipeIngredients.AddRecipeIngredient;
using Home.WebApi.Presenters.RecipeIngredients.GetIngredientSuggestions;
using Home.WebApi.Presenters.RecipeIngredients.GetRecipeIngredient;
using Home.WebApi.Presenters.RecipeIngredients.RemoveRecipeIngredient;
using Home.WebApi.Presenters.RecipeIngredients.SetRecipeIngredientSequence;
using Home.WebApi.Presenters.RecipeIngredients.UpdateRecipeIngredient;
using Home.WebApi.UseCases.RecipeIngredients.AddRecipeIngredient;
using Home.WebApi.UseCases.RecipeIngredients.GetIngredientSuggestions;
using Home.WebApi.UseCases.RecipeIngredients.GetRecipeIngredient;
using Home.WebApi.UseCases.RecipeIngredients.SetRecipeIngredientSequence;
using Home.WebApi.UseCases.RecipeIngredients.UpdateRecipeIngredient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class RecipeIngredientsController : BaseController
{

    #region Methods

    [HttpPost]
    [ProducesResponseType<AddRecipeIngredientApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddRecipeIngredient(
        [FromServices] AddRecipeIngredientPresenter presenter,
        [FromBody] AddRecipeIngredientApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(
            new AddRecipeIngredientInputPort(request.Amount, request.Name, request.RecipeID, request.Unit),
            presenter,
            this.ServiceFactory,
            cancellationToken);

        return presenter.Result;
    }

    [HttpGet("suggestions")]
    [ProducesResponseType<GetIngredientSuggestionsApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIngredientSuggestions(
        [FromServices] GetIngredientSuggestionsPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetIngredientSuggestionsInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet("{ingredientID}")]
    [ProducesResponseType<GetRecipeIngredientApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecipeIngredient(
        [FromServices] GetRecipeIngredientPresenter presenter,
        [FromRoute] long ingredientID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetRecipeIngredientInputPort(ingredientID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpDelete("{recipeID}/{ingredientID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveRecipeIngredient(
        [FromServices] RemoveRecipeIngredientPresenter presenter,
        [FromRoute] long recipeID,
        [FromRoute] long ingredientID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new RemoveRecipeIngredientInputPort(ingredientID, recipeID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{recipeID}/{ingredientID}/sequence")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetRecipeIngredientSequence(
        [FromServices] SetRecipeIngredientSequencePresenter presenter,
        [FromRoute] long recipeID,
        [FromRoute] long ingredientID,
        [FromBody] SetRecipeIngredientSequenceApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new SetRecipeIngredientSequenceInputPort(ingredientID, recipeID, request.Sequence), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{ingredientID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateRecipeIngredient(
        [FromServices] UpdateRecipeIngredientPresenter presenter,
        [FromRoute] long ingredientID,
        [FromBody] UpdateRecipeIngredientApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(
            new UpdateRecipeIngredientInputPort(request.Amount, ingredientID, request.Name, request.Unit),
            presenter,
            this.ServiceFactory,
            cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
