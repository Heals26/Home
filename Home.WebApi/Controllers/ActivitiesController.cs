using Home.Application.UseCases.Activities.CreateActivity;
using Home.Application.UseCases.Activities.DeleteActivity;
using Home.Application.UseCases.Activities.GetActivities;
using Home.Application.UseCases.Activities.GetActivity;
using Home.Application.UseCases.Activities.GetAssignedActivities;
using Home.Application.UseCases.Activities.SetActivityCompletion;
using Home.Application.UseCases.Activities.SetActivityTags;
using Home.Application.UseCases.Activities.UpdateActivity;
using Home.Application.UseCases.ActivityRegions.GetActivityRegions;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.Activities.CreateActivity;
using Home.WebApi.Presenters.Activities.DeleteActivity;
using Home.WebApi.Presenters.Activities.GetActivities;
using Home.WebApi.Presenters.Activities.GetActivity;
using Home.WebApi.Presenters.Activities.GetAssignedActivities;
using Home.WebApi.Presenters.Activities.SetActivityCompletion;
using Home.WebApi.Presenters.Activities.SetActivityTags;
using Home.WebApi.Presenters.Activities.UpdateActivity;
using Home.WebApi.Presenters.ActivityRegions.GetActivityRegions;
using Home.WebApi.UseCases.Activities.CreateActivity;
using Home.WebApi.UseCases.Activities.GetActivities;
using Home.WebApi.UseCases.Activities.SetActivityCompletion;
using Home.WebApi.UseCases.Activities.GetActivity;
using Home.WebApi.UseCases.Activities.SetActivityTags;
using Home.WebApi.UseCases.Activities.UpdateActivity;
using Home.WebApi.UseCases.ActivityRegions.GetActivityRegions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class ActivitiesController : BaseController
{

    #region Methods

    [HttpPost]
    [ProducesResponseType<CreateActivityApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateActivity(
        [FromServices] CreateActivityPresenter presenter,
        [FromBody] CreateActivityApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(
            new CreateActivityInputPort(request.Title, request.DueDateUTC, request.DueTime, request.StateID, request.UserID),
            presenter,
            this.ServiceFactory,
            cancellationToken);

        return presenter.Result;
    }

    [HttpDelete("{activityID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteActivity(
        [FromServices] DeleteActivityPresenter presenter,
        [FromRoute] long activityID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteActivityInputPort(activityID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet("{activityID}")]
    [ProducesResponseType<GetActivityApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivity(
        [FromServices] GetActivityPresenter presenter,
        [FromRoute] long activityID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetActivityInputPort(activityID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet]
    [ProducesResponseType<GetActivitiesApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivities(
        [FromServices] GetActivitiesPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetActivitiesInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet("Assigned")]
    [ProducesResponseType<GetActivitiesApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignedActivities(
        [FromServices] GetAssignedActivitiesPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetAssignedActivitiesInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet("{activityID}/Regions")]
    [ProducesResponseType<GetActivityRegionsApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivityRegions(
        [FromServices] GetActivityRegionsPresenter presenter,
        [FromRoute] long activityID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetActivityRegionsInputPort(activityID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    /// <summary>
    /// Replaces the card's tags with the set sent. Anything left out is taken off the card.
    /// </summary>
    [HttpPut("{activityID}/Tags")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetActivityTags(
        [FromServices] SetActivityTagsPresenter presenter,
        [FromRoute] long activityID,
        [FromBody] SetActivityTagsApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new SetActivityTagsInputPort(activityID, request.TagIDs), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{activityID}/Completion")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetActivityCompletion(
        [FromServices] SetActivityCompletionPresenter presenter,
        [FromRoute] long activityID,
        [FromBody] SetActivityCompletionApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new SetActivityCompletionInputPort(activityID, request.IsComplete), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{activityID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateActivity(
        [FromServices] UpdateActivityPresenter presenter,
        [FromRoute] long activityID,
        [FromBody] UpdateActivityApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(
            new UpdateActivityInputPort(activityID, request.Title, request.DueDateUTC, request.DueTime, request.CompletedDateUTC, request.Sequence, request.StateID, request.UserID),
            presenter,
            this.ServiceFactory,
            cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
