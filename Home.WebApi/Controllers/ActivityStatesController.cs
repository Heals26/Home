using Home.Application.UseCases.ActivityStates.CreateActivityState;
using Home.Application.UseCases.ActivityStates.DeleteActivityState;
using Home.Application.UseCases.ActivityStates.GetActivityStates;
using Home.Application.UseCases.ActivityStates.UpdateActivityState;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.ActivityStates.CreateActivityState;
using Home.WebApi.Presenters.ActivityStates.DeleteActivityState;
using Home.WebApi.Presenters.ActivityStates.GetActivityStates;
using Home.WebApi.Presenters.ActivityStates.UpdateActivityState;
using Home.WebApi.UseCases.ActivityStates.CreateActivityState;
using Home.WebApi.UseCases.ActivityStates.GetActivityStates;
using Home.WebApi.UseCases.ActivityStates.UpdateActivityState;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class ActivityStatesController : BaseController
{

    #region Methods

    [HttpPost]
    [ProducesResponseType<CreateActivityStateApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateActivityState(
        [FromServices] CreateActivityStatePresenter presenter,
        [FromBody] CreateActivityStateApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new CreateActivityStateInputPort(request.Name, request.IsComplete), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    /// <summary>
    /// Deletes a column, moving every card in it to moveCardsToStateID first. Returns 409 when the
    /// column is the last one on the board, or when a card still points at it.
    /// </summary>
    [HttpDelete("{activityStateID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DeleteActivityState(
        [FromServices] DeleteActivityStatePresenter presenter,
        [FromRoute] long activityStateID,
        [FromQuery] long moveCardsToStateID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteActivityStateInputPort(activityStateID, moveCardsToStateID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet]
    [ProducesResponseType<GetActivityStatesApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivityStates(
        [FromServices] GetActivityStatesPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetActivityStatesInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{activityStateID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateActivityState(
        [FromServices] UpdateActivityStatePresenter presenter,
        [FromRoute] long activityStateID,
        [FromBody] UpdateActivityStateApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new UpdateActivityStateInputPort(activityStateID, request.IsComplete, request.Name, request.Sequence), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
