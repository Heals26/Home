using CleanArchitecture.Mediator;
using Home.Application.UseCases.Calendar.Models;
using Home.Domain.Enumerations;

namespace Home.Application.UseCases.Calendar.CreateCalendarEvent;

/// <summary>
/// Puts something on the household's calendar. Times are wall-clock in <paramref name="TimeZoneID"/>,
/// the zone of the browser that made it; both are ignored for an all-day event.
/// </summary>
public record CreateCalendarEventInputPort(
    int DaysOfWeek,
    DateOnly EndDate,
    TimeOnly? EndTime,
    CalendarRecurrenceFrequency Frequency,
    int Interval,
    bool IsAllDay,
    string? Location,
    IReadOnlyList<long> MemberUserIDs,
    string? Notes,
    bool RepeatsOnWeekdayOfMonth,
    DateOnly? RepeatUntil,
    DateOnly StartDate,
    TimeOnly? StartTime,
    string? TimeZoneID,
    string Title)
    : IInputPort<ICreateCalendarEventOutputPort>, ICalendarEventInput;
