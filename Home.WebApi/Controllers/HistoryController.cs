using Home.Application.UseCases.History.GetEntityHistory;
using Home.Application.UseCases.History.GetHistory;
using Home.Application.UseCases.History.Models;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.History.GetEntityHistory;
using Home.WebApi.Presenters.History.GetHistory;
using Home.WebApi.UseCases.History.GetEntityHistory;
using Home.WebApi.UseCases.History.GetHistory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class HistoryController : BaseController
{

    #region Methods

    [HttpGet("{resourceTypeValue}/{entityID}")]
    [ProducesResponseType<GetEntityHistoryApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEntityHistory(
        [FromServices] GetEntityHistoryPresenter presenter,
        [FromRoute] long resourceTypeValue,
        [FromRoute] long entityID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetEntityHistoryInputPort(entityID, resourceTypeValue), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    /// <summary>
    /// A page of the household's feed, newest first. Repeat <c>categories</c> to narrow it; leave
    /// it out for everything.
    /// </summary>
    [HttpGet]
    [ProducesResponseType<GetHistoryApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(
        [FromServices] GetHistoryPresenter presenter,
        [FromQuery] long? beforeAuditID,
        [FromQuery] int take,
        [FromQuery] HistoryCategory[] categories,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetHistoryInputPort(beforeAuditID, categories ?? [], take == 0 ? 25 : take), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
