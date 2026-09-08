using Home.WebUI.DataAccess.Calendar.Models;

namespace Home.WebUI.DataAccess.Calendar.GetCalendarEvent;

/// <summary>
/// An event as stored, rule and all, which is what the editor works from.
/// </summary>
public class GetCalendarEventWebAppResponse
{

    #region Properties

    public long CalendarEventID { get; set; }
    public int DaysOfWeek { get; set; }
    public DateOnly EndDate { get; set; }
    public TimeOnly? EndTime { get; set; }
    public CalendarRecurrenceFrequency Frequency { get; set; }
    public int Interval { get; set; }
    public bool IsAllDay { get; set; }

    /// <summary>
    /// True when the event came from a subscribed calendar and can only be changed there.
    /// </summary>
    public bool IsReadOnly { get; set; }

    public string? Location { get; set; }
    public List<long> MemberUserIDs { get; set; } = [];
    public string? Notes { get; set; }
    public bool RepeatsOnWeekdayOfMonth { get; set; }
    public DateOnly? RepeatUntil { get; set; }
    public DateOnly StartDate { get; set; }
    public TimeOnly? StartTime { get; set; }
    public string? SubscriptionName { get; set; }
    public string? TimeZoneID { get; set; }
    public string Title { get; set; } = string.Empty;

    #endregion Properties

}
