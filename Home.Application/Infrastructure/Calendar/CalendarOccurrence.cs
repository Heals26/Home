using Home.Domain.Entities;

namespace Home.Application.Infrastructure.Calendar;

/// <summary>
/// One time an event happens. Dates are in the event's own zone; the instants are UTC and null
/// for an all-day event, because a date is not a moment.
/// </summary>
public record CalendarOccurrence(
    CalendarEvent Event,
    DateOnly StartDate,
    DateOnly EndDate,
    DateTime? StartUTC,
    DateTime? EndUTC);
