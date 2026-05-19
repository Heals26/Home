using Home.Application.UseCases.RecipeNotes.AddRecipeNote;
using Home.Application.UseCases.RecipeNotes.RemoveRecipeNote;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.RecipeNotes.AddRecipeNote;
using Home.WebApi.Presenters.RecipeNotes.RemoveRecipeNote;
using Home.WebApi.UseCases.RecipeNotes.AddRecipeNote;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class RecipeNotesController : BaseController
{

    #region Methods

    [HttpPost]
    [ProducesResponseType<AddRecipeNoteApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddRecipeNote(
        [FromServices] AddRecipeNotePresenter presenter,
        [FromBody] AddRecipeNoteApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new AddRecipeNoteInputPort(request.Content, request.RecipeID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpDelete("{recipeID}/{noteID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveRecipeNote(
        [FromServices] RemoveRecipeNotePresenter presenter,
        [FromRoute] long recipeID,
        [FromRoute] long noteID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new RemoveRecipeNoteInputPort(noteID, recipeID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
