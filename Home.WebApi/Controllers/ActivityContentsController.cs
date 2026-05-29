using Home.Application.UseCases.ActivityContents.CreateActivityContent;
using Home.Application.UseCases.ActivityContents.DeleteActivityContent;
using Home.Application.UseCases.ActivityContents.GetActivityContent;
using Home.Application.UseCases.ActivityContents.UpdateActivityContent;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.ActivityContents.CreateActivityContent;
using Home.WebApi.Presenters.ActivityContents.DeleteActivityContent;
using Home.WebApi.Presenters.ActivityContents.GetActivityContent;
using Home.WebApi.Presenters.ActivityContents.UpdateActivityContent;
using Home.WebApi.UseCases.ActivityContents.CreateActivityContent;
using Home.WebApi.UseCases.ActivityContents.GetActivityContent;
using Home.WebApi.UseCases.ActivityContents.UpdateActivityContent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class ActivityContentsController : BaseController
{

    #region Methods

    [HttpPost]
    [ProducesResponseType<CreateActivityContentApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateActivityContent(
        [FromServices] CreateActivityContentPresenter presenter,
        [FromBody] CreateActivityContentApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(
            new CreateActivityContentInputPort(request.ActivityRegionID, request.Content),
            presenter,
            this.ServiceFactory,
            cancellationToken);

        return presenter.Result;
    }

    [HttpDelete("{activityContentID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteActivityContent(
        [FromServices] DeleteActivityContentPresenter presenter,
        [FromRoute] long activityContentID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteActivityContentInputPort(activityContentID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet("{activityContentID}")]
    [ProducesResponseType<GetActivityContentApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivityContent(
        [FromServices] GetActivityContentPresenter presenter,
        [FromRoute] long activityContentID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetActivityContentInputPort(activityContentID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{activityContentID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateActivityContent(
        [FromServices] UpdateActivityContentPresenter presenter,
        [FromRoute] long activityContentID,
        [FromBody] UpdateActivityContentApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(
            new UpdateActivityContentInputPort(activityContentID, request.Content, request.Sequence),
            presenter,
            this.ServiceFactory,
            cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
