using Home.Application.UseCases.CalendarSubscriptions.CreateCalendarSubscription;
using Home.Application.UseCases.CalendarSubscriptions.DeleteCalendarSubscription;
using Home.Application.UseCases.CalendarSubscriptions.GetCalendarSubscriptions;
using Home.Application.UseCases.CalendarSubscriptions.RefreshCalendarSubscription;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.CalendarSubscriptions.CreateCalendarSubscription;
using Home.WebApi.Presenters.CalendarSubscriptions.DeleteCalendarSubscription;
using Home.WebApi.Presenters.CalendarSubscriptions.GetCalendarSubscriptions;
using Home.WebApi.Presenters.CalendarSubscriptions.RefreshCalendarSubscription;
using Home.WebApi.UseCases.CalendarSubscriptions.CreateCalendarSubscription;
using Home.WebApi.UseCases.CalendarSubscriptions.GetCalendarSubscriptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class CalendarSubscriptionsController : BaseController
{

    #region Methods

    [HttpPost]
    [ProducesResponseType<CreateCalendarSubscriptionApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateCalendarSubscription(
        [FromServices] CreateCalendarSubscriptionPresenter presenter,
        [FromBody] CreateCalendarSubscriptionApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new CreateCalendarSubscriptionInputPort(request.Name ?? string.Empty, request.Url ?? string.Empty), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpDelete("{calendarSubscriptionID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteCalendarSubscription(
        [FromServices] DeleteCalendarSubscriptionPresenter presenter,
        [FromRoute] long calendarSubscriptionID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteCalendarSubscriptionInputPort(calendarSubscriptionID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet]
    [ProducesResponseType<GetCalendarSubscriptionsApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalendarSubscriptions(
        [FromServices] GetCalendarSubscriptionsPresenter presenter,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetCalendarSubscriptionsInputPort(), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPost("{calendarSubscriptionID}/refresh")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RefreshCalendarSubscription(
        [FromServices] RefreshCalendarSubscriptionPresenter presenter,
        [FromRoute] long calendarSubscriptionID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new RefreshCalendarSubscriptionInputPort(calendarSubscriptionID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
