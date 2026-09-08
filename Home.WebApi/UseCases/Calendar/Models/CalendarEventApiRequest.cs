using Home.Domain.Enumerations;

namespace Home.WebApi.UseCases.Calendar.Models;

/// <summary>
/// The whole of an event, as create and update both take it. Times are wall-clock in
/// <c>TimeZoneID</c>, the zone of the browser filling the form; both are ignored when all-day.
/// </summary>
public record CalendarEventApiRequest(
    int DaysOfWeek,
    DateOnly EndDate,
    TimeOnly? EndTime,
    CalendarRecurrenceFrequency Frequency,
    int Interval,
    bool IsAllDay,
    string Location,
    List<long> MemberUserIDs,
    string Notes,
    bool RepeatsOnWeekdayOfMonth,
    DateOnly? RepeatUntil,
    DateOnly StartDate,
    TimeOnly? StartTime,
    string TimeZoneID,
    string Title);
