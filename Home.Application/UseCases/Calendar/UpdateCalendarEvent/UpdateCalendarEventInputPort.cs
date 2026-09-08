using CleanArchitecture.Mediator;
using Home.Application.UseCases.Calendar.Models;
using Home.Domain.Enumerations;

namespace Home.Application.UseCases.Calendar.UpdateCalendarEvent;

/// <summary>
/// Replaces the whole event. Not a partial update on purpose: the editor always has every field,
/// and the recurrence fields depend on one another, so a change to one cannot be judged alone.
/// </summary>
public record UpdateCalendarEventInputPort(
    long CalendarEventID,
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
    : IInputPort<IUpdateCalendarEventOutputPort>, ICalendarEventInput;
