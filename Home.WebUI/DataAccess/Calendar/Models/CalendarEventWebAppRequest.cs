namespace Home.WebUI.DataAccess.Calendar.Models;

/// <summary>
/// The whole of an event, as create and update both send it. Times are wall-clock in
/// <see cref="TimeZoneID"/>, the browser's zone; both are ignored when all-day.
/// </summary>
public class CalendarEventWebAppRequest
{

    #region Properties

    /// <summary>
    /// Weekly only: a bitmask with bit 0 as Sunday.
    /// </summary>
    public int DaysOfWeek { get; set; }

    public DateOnly EndDate { get; set; }
    public TimeOnly? EndTime { get; set; }
    public CalendarRecurrenceFrequency Frequency { get; set; }

    /// <summary>
    /// Every N periods of the frequency. One for an ordinary repeat.
    /// </summary>
    public int Interval { get; set; } = 1;

    public bool IsAllDay { get; set; }
    public string? Location { get; set; }
    public List<long> MemberUserIDs { get; set; } = [];
    public string? Notes { get; set; }

    /// <summary>
    /// Monthly only: repeat on the same weekday of the month ("first Monday") rather than the date.
    /// </summary>
    public bool RepeatsOnWeekdayOfMonth { get; set; }

    public DateOnly? RepeatUntil { get; set; }
    public DateOnly StartDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public string? TimeZoneID { get; set; }
    public string Title { get; set; } = string.Empty;

    #endregion Properties

}
