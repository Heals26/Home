using AutoMapper;
using Home.Application.UseCases.Calendar.GetCalendarEvent;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Calendar.GetCalendarEvent;

namespace Home.WebApi.Presenters.Calendar.GetCalendarEvent;

public class GetCalendarEventPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetCalendarEventOutputPort
{

    #region Methods

    Task IGetCalendarEventOutputPort.PresentCalendarEventAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken)
        => this.OkAsync(new GetCalendarEventApiResponse()
        {
            CalendarEventID = calendarEvent.CalendarEventID,
            DaysOfWeek = calendarEvent.DaysOfWeek,
            EndDate = calendarEvent.EndDate,
            EndTime = calendarEvent.EndTime,
            Frequency = calendarEvent.Frequency,
            Interval = calendarEvent.Interval,
            IsAllDay = calendarEvent.IsAllDay,
            IsReadOnly = calendarEvent.Subscription != null,
            Location = calendarEvent.Location,
            MemberUserIDs = [.. calendarEvent.Members.Select(m => m.UserID)],
            Notes = calendarEvent.Notes,
            RepeatsOnWeekdayOfMonth = calendarEvent.RepeatsOnWeekdayOfMonth,
            RepeatUntil = calendarEvent.RepeatUntil,
            StartDate = calendarEvent.StartDate,
            StartTime = calendarEvent.StartTime,
            SubscriptionName = calendarEvent.Subscription?.Name,
            TimeZoneID = calendarEvent.TimeZoneID,
            Title = calendarEvent.Title
        }, cancellationToken);

    Task IGetCalendarEventOutputPort.PresentCalendarEventNotFoundAsync(long calendarEventID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Calendar Event {calendarEventID} Not Found", cancellationToken);

    #endregion Methods

}
