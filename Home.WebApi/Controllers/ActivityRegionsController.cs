using Home.Application.UseCases.ActivityContents.GetActivityContents;
using Home.Application.UseCases.ActivityRegions.CreateActivityRegion;
using Home.Application.UseCases.ActivityRegions.DeleteActivityRegion;
using Home.Application.UseCases.ActivityRegions.GetActivityRegion;
using Home.Application.UseCases.ActivityRegions.UpdateActivityRegion;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.ActivityContents.GetActivityContents;
using Home.WebApi.Presenters.ActivityRegions.CreateActivityRegion;
using Home.WebApi.Presenters.ActivityRegions.DeleteActivityRegion;
using Home.WebApi.Presenters.ActivityRegions.GetActivityRegion;
using Home.WebApi.Presenters.ActivityRegions.UpdateActivityRegion;
using Home.WebApi.UseCases.ActivityContents.GetActivityContents;
using Home.WebApi.UseCases.ActivityRegions.CreateActivityRegion;
using Home.WebApi.UseCases.ActivityRegions.GetActivityRegion;
using Home.WebApi.UseCases.ActivityRegions.UpdateActivityRegion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class ActivityRegionsController : BaseController
{

    #region Methods

    [HttpPost]
    [ProducesResponseType<CreateActivityRegionApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateActivityRegion(
        [FromServices] CreateActivityRegionPresenter presenter,
        [FromBody] CreateActivityRegionApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(
            new CreateActivityRegionInputPort(request.ActivityID, request.Region),
            presenter,
            this.ServiceFactory,
            cancellationToken);

        return presenter.Result;
    }

    [HttpDelete("{activityRegionID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteActivityRegion(
        [FromServices] DeleteActivityRegionPresenter presenter,
        [FromRoute] long activityRegionID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteActivityRegionInputPort(activityRegionID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet("{activityRegionID}")]
    [ProducesResponseType<GetActivityRegionApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivityRegion(
        [FromServices] GetActivityRegionPresenter presenter,
        [FromRoute] long activityRegionID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetActivityRegionInputPort(activityRegionID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet("{activityRegionID}/Contents")]
    [ProducesResponseType<GetActivityContentsApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivityContents(
        [FromServices] GetActivityContentsPresenter presenter,
        [FromRoute] long activityRegionID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetActivityContentsInputPort(activityRegionID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPatch("{activityRegionID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateActivityRegion(
        [FromServices] UpdateActivityRegionPresenter presenter,
        [FromRoute] long activityRegionID,
        [FromBody] UpdateActivityRegionApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(
            new UpdateActivityRegionInputPort(activityRegionID, request.Region, request.Sequence),
            presenter,
            this.ServiceFactory,
            cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
