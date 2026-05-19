using Home.Application.UseCases.IngredientNotes.AddIngredientNote;
using Home.Application.UseCases.IngredientNotes.RemoveIngredientNote;
using Home.Application.UseCases.Notes.UpdateNote;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.IngredientNotes.AddIngredientNote;
using Home.WebApi.Presenters.IngredientNotes.RemoveIngredientNote;
using Home.WebApi.Presenters.Notes.UpdateNote;
using Home.WebApi.UseCases.IngredientNotes.AddIngredientNote;
using Home.WebApi.UseCases.Notes.UpdateNote;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class IngredientNotesController : BaseController
{

    #region Methods

    [HttpPost]
    [ProducesResponseType<AddIngredientNoteApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> AddIngredientNote(
        [FromServices] AddIngredientNotePresenter presenter,
        [FromBody] AddIngredientNoteApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new AddIngredientNoteInputPort(request.Content, request.IngredientID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpDelete("{ingredientID}/Note/{noteID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveIngredientNote(
        [FromServices] RemoveIngredientNotePresenter presenter,
        [FromRoute] long ingredientID,
        [FromRoute] long noteID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new RemoveIngredientNoteInputPort(ingredientID, noteID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{ingredientNoteID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateNote(
        [FromServices] UpdateNotePresenter presenter,
        [FromRoute] long ingredientNoteID,
        [FromBody] UpdateNoteApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new UpdateNoteInputPort(ingredientNoteID, request.Content), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
