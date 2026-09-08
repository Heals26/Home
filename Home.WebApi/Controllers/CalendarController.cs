using Home.Application.UseCases.Calendar.CreateCalendarEvent;
using Home.Application.UseCases.Calendar.DeleteCalendarEvent;
using Home.Application.UseCases.Calendar.GetCalendar;
using Home.Application.UseCases.Calendar.GetCalendarEvent;
using Home.Application.UseCases.Calendar.UpdateCalendarEvent;
using Home.WebApi.Infrastructure.Attributes;
using Home.WebApi.Infrastructure.Values;
using Home.WebApi.Presenters.Calendar.CreateCalendarEvent;
using Home.WebApi.Presenters.Calendar.DeleteCalendarEvent;
using Home.WebApi.Presenters.Calendar.GetCalendar;
using Home.WebApi.Presenters.Calendar.GetCalendarEvent;
using Home.WebApi.Presenters.Calendar.UpdateCalendarEvent;
using Home.WebApi.UseCases.Calendar.CreateCalendarEvent;
using Home.WebApi.UseCases.Calendar.GetCalendar;
using Home.WebApi.UseCases.Calendar.GetCalendarEvent;
using Home.WebApi.UseCases.Calendar.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Home.WebApi.Controllers;

[Version1]
[Route("api/[controller]")]
[Authorize(Policy = FrameworkValues.ScopeWebApp)]
public class CalendarController : BaseController
{

    #region Methods

    [HttpPost("Events")]
    [ProducesResponseType<CreateCalendarEventApiResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateCalendarEvent(
        [FromServices] CreateCalendarEventPresenter presenter,
        [FromBody] CalendarEventApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new CreateCalendarEventInputPort(request.DaysOfWeek, request.EndDate, request.EndTime, request.Frequency, request.Interval, request.IsAllDay, request.Location, request.MemberUserIDs ?? [], request.Notes, request.RepeatsOnWeekdayOfMonth, request.RepeatUntil, request.StartDate, request.StartTime, request.TimeZoneID, request.Title ?? string.Empty), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    /// <summary>
    /// Without <paramref name="occurrenceDate"/> the whole event goes; with it, on a repeating
    /// event, only that occurrence is skipped.
    /// </summary>
    [HttpDelete("Events/{calendarEventID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteCalendarEvent(
        [FromServices] DeleteCalendarEventPresenter presenter,
        [FromRoute] long calendarEventID,
        [FromQuery] DateOnly? occurrenceDate,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new DeleteCalendarEventInputPort(calendarEventID, occurrenceDate), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet]
    [ProducesResponseType<GetCalendarApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalendar(
        [FromServices] GetCalendarPresenter presenter,
        [FromQuery] DateOnly fromDate,
        [FromQuery] DateOnly toDate,
        [FromQuery] string timeZoneID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetCalendarInputPort(fromDate, timeZoneID ?? string.Empty, toDate), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpGet("Events/{calendarEventID}")]
    [ProducesResponseType<GetCalendarEventApiResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalendarEvent(
        [FromServices] GetCalendarEventPresenter presenter,
        [FromRoute] long calendarEventID,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new GetCalendarEventInputPort(calendarEventID), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    [HttpPut("Events/{calendarEventID}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateCalendarEvent(
        [FromServices] UpdateCalendarEventPresenter presenter,
        [FromRoute] long calendarEventID,
        [FromBody] CalendarEventApiRequest request,
        CancellationToken cancellationToken)
    {
        await this.Pipeline.InvokeAsync(new UpdateCalendarEventInputPort(calendarEventID, request.DaysOfWeek, request.EndDate, request.EndTime, request.Frequency, request.Interval, request.IsAllDay, request.Location, request.MemberUserIDs ?? [], request.Notes, request.RepeatsOnWeekdayOfMonth, request.RepeatUntil, request.StartDate, request.StartTime, request.TimeZoneID, request.Title ?? string.Empty), presenter, this.ServiceFactory, cancellationToken);

        return presenter.Result;
    }

    #endregion Methods

}
