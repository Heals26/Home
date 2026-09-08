using Home.Domain.Enumerations;

namespace Home.Application.UseCases.Calendar.Models;

/// <summary>
/// Everything a caller says about an event, shared by create and update so the two cannot drift
/// in what they accept. The editor always sends the whole event: the recurrence fields depend on
/// each other, so a partial update could never be validated on its own.
/// </summary>
public interface ICalendarEventInput
{

    #region Properties

    int DaysOfWeek { get; }
    DateOnly EndDate { get; }
    TimeOnly? EndTime { get; }
    CalendarRecurrenceFrequency Frequency { get; }
    int Interval { get; }
    bool IsAllDay { get; }
    string? Location { get; }
    IReadOnlyList<long> MemberUserIDs { get; }
    string? Notes { get; }
    bool RepeatsOnWeekdayOfMonth { get; }
    DateOnly? RepeatUntil { get; }
    DateOnly StartDate { get; }
    TimeOnly? StartTime { get; }
    string? TimeZoneID { get; }
    string Title { get; }

    #endregion Properties

}
