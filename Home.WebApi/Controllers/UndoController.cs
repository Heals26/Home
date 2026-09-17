using Home.Application.UseCases.Undo.UndoAction;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.Undo.UndoAction;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class UndoController : BaseController
{

    #region Methods

    [HttpPost("{token}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UndoAction(
        [FromServices] UndoActionPresenter presenter,
        [FromRoute] Guid token,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new UndoActionInputPort(token), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
