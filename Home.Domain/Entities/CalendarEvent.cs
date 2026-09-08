using Home.Domain.Enumerations;

namespace Home.Domain.Entities;

/// <summary>
/// Something on the household's calendar that belongs to nothing else: an appointment, a lesson,
/// a birthday. Planned meals and tasks are not copied in here; the calendar read folds them in.
/// <para>
/// Time is held as the family wrote it: a date, and for a timed event a wall-clock time plus the
/// zone it was created in. The API turns that into UTC instants when it reads, so a repeating
/// 4pm stays 4pm across daylight saving and a viewer in another zone still sees the right day.
/// </para>
/// </summary>
public class CalendarEvent
{

    #region Properties

    public long CalendarEventID { get; set; }

    /// <summary>
    /// Weekly only. Bit 0 is Sunday, matching <see cref="DayOfWeek"/> and <c>LightSchedule</c>.
    /// Zero on a weekly event means the start date's own weekday.
    /// </summary>
    public int DaysOfWeek { get; set; }

    /// <summary>
    /// Last day of the event (inclusive). Equal to <see cref="StartDate"/> for a single day.
    /// </summary>
    public DateOnly EndDate { get; set; }

    /// <summary>
    /// Wall-clock end on <see cref="EndDate"/>, in <see cref="TimeZoneID"/>. Null when all-day.
    /// </summary>
    public TimeOnly? EndTime { get; set; }

    /// <summary>
    /// The feed's identifier for an occurrence that came from a subscription. Null on Home's own
    /// events.
    /// </summary>
    public string? ExternalUID { get; set; }

    public CalendarRecurrenceFrequency Frequency { get; set; }

    /// <summary>
    /// Every N periods of <see cref="Frequency"/>. One for an ordinary repeat.
    /// </summary>
    public int Interval { get; set; } = 1;

    public bool IsAllDay { get; set; }

    public string? Location { get; set; }

    public string? Notes { get; set; }

    /// <summary>
    /// Monthly only. True repeats on the same weekday of the month as the start date ("first
    /// Monday"); false repeats on the same day of the month.
    /// </summary>
    public bool RepeatsOnWeekdayOfMonth { get; set; }

    /// <summary>
    /// Last day an occurrence may start. Null repeats forever.
    /// </summary>
    public DateOnly? RepeatUntil { get; set; }

    /// <summary>
    /// First day of the event, and the anchor every repeat is counted from. For a timed event
    /// this is the local date in <see cref="TimeZoneID"/>.
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Wall-clock start in <see cref="TimeZoneID"/>. Null when all-day.
    /// </summary>
    public TimeOnly? StartTime { get; set; }

    /// <summary>
    /// IANA zone the times were written in, taken from the browser that created the event. Null
    /// when all-day, because a date has no zone.
    /// </summary>
    public string? TimeZoneID { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Occurrences of a series that have been skipped or moved away.
    /// </summary>
    public ICollection<CalendarEventException> Exceptions { get; set; } = [];

    public Household Household { get; set; } = null!;

    /// <summary>
    /// Who is on it. Display and filtering, not ownership: the household owns every event.
    /// </summary>
    public ICollection<CalendarEventMember> Members { get; set; } = [];

    /// <summary>
    /// The feed this occurrence was read from, or null for Home's own events. A subscribed event
    /// is read-only in Home.
    /// </summary>
    public CalendarSubscription? Subscription { get; set; }

    #endregion Properties

}
