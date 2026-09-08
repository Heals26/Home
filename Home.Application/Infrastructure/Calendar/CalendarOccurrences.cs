using Home.Domain.Entities;

namespace Home.Application.Infrastructure.Calendar;

/// <summary>
/// Expands an event into its occurrences inside a window, resolving each timed one to UTC
/// through the zone the event was written in. This is the only place wall-clock becomes instant.
/// </summary>
public static class CalendarOccurrences
{

    #region Methods

    public static IEnumerable<CalendarOccurrence> Expand(CalendarEvent calendarEvent, DateOnly windowStart, DateOnly windowEnd)
    {
        var _Length = Math.Max(0, calendarEvent.EndDate.DayNumber - calendarEvent.StartDate.DayNumber);
        var _IsTimed = !calendarEvent.IsAllDay && calendarEvent.StartTime != null && calendarEvent.EndTime != null;
        var _Zone = _IsTimed ? TimeZoneResolver.Resolve(calendarEvent.TimeZoneID) : null;

        foreach (var _StartDate in RecurrenceExpander.StartDates(calendarEvent, windowStart, windowEnd))
        {
            var _EndDate = _StartDate.AddDays(_Length);

            yield return _IsTimed
                ? new CalendarOccurrence(
                    calendarEvent,
                    _StartDate,
                    _EndDate,
                    TimeZoneResolver.ToUtc(_StartDate, calendarEvent.StartTime!.Value, _Zone!),
                    TimeZoneResolver.ToUtc(_EndDate, calendarEvent.EndTime!.Value, _Zone!))
                : new CalendarOccurrence(calendarEvent, _StartDate, _EndDate, null, null);
        }
    }

    #endregion Methods

}
