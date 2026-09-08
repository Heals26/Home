using Home.WebUI.DataAccess.Calendar.GetCalendarEvent;
using Home.WebUI.DataAccess.Calendar.Models;
using Home.WebUI.Infrastructure.Calendar;

namespace Home.WebUI.Components.Pages.Calendar.Models;

/// <summary>
/// What the event editor binds to. Dates and times are held as the strings the browser's date
/// and time inputs speak, and turned into a request once, on save, so a half-typed field never
/// throws mid-edit.
/// </summary>
public class CalendarEventForm
{

    #region Properties

    public int DaysOfWeek { get; set; }
    public string EndDate { get; set; } = string.Empty;
    public string EndTime { get; set; } = "10:00";
    public CalendarRecurrenceFrequency Frequency { get; set; }
    public int Interval { get; set; } = 1;
    public bool IsAllDay { get; set; }
    public string Location { get; set; } = string.Empty;
    public HashSet<long> MemberUserIDs { get; set; } = [];
    public string Notes { get; set; } = string.Empty;
    public bool RepeatsOnWeekdayOfMonth { get; set; }
    public string RepeatUntil { get; set; } = string.Empty;
    public string StartDate { get; set; } = string.Empty;
    public string StartTime { get; set; } = "09:00";
    public string Title { get; set; } = string.Empty;

    #endregion Properties

    #region Methods

    /// <summary>
    /// A blank event on the given day, timed at nine, because a tap on a day usually means "put
    /// something here" rather than "block the whole day".
    /// </summary>
    public static CalendarEventForm ForNew(DateOnly date)
        => new()
        {
            EndDate = CalendarFormat.IsoDate(date),
            StartDate = CalendarFormat.IsoDate(date)
        };

    public static CalendarEventForm FromEvent(GetCalendarEventWebAppResponse calendarEvent)
        => new()
        {
            DaysOfWeek = calendarEvent.DaysOfWeek,
            EndDate = CalendarFormat.IsoDate(calendarEvent.EndDate),
            EndTime = calendarEvent.EndTime is { } _EndTime ? CalendarFormat.IsoTime(_EndTime) : "10:00",
            Frequency = calendarEvent.Frequency,
            Interval = Math.Max(1, calendarEvent.Interval),
            IsAllDay = calendarEvent.IsAllDay,
            Location = calendarEvent.Location ?? string.Empty,
            MemberUserIDs = [.. calendarEvent.MemberUserIDs],
            Notes = calendarEvent.Notes ?? string.Empty,
            RepeatsOnWeekdayOfMonth = calendarEvent.RepeatsOnWeekdayOfMonth,
            RepeatUntil = calendarEvent.RepeatUntil is { } _Until ? CalendarFormat.IsoDate(_Until) : string.Empty,
            StartDate = CalendarFormat.IsoDate(calendarEvent.StartDate),
            StartTime = calendarEvent.StartTime is { } _StartTime ? CalendarFormat.IsoTime(_StartTime) : "09:00",
            Title = calendarEvent.Title
        };

    /// <summary>
    /// One occurrence of a series, detached: the same words at the same time on that one day,
    /// with no repeat, so it can be moved without touching the rest.
    /// </summary>
    public static CalendarEventForm FromOccurrence(CalendarItemDto item, GetCalendarEventWebAppResponse series)
    {
        var _Form = FromEvent(series);

        _Form.Frequency = CalendarRecurrenceFrequency.None;
        _Form.DaysOfWeek = 0;
        _Form.Interval = 1;
        _Form.RepeatUntil = string.Empty;
        _Form.RepeatsOnWeekdayOfMonth = false;

        var _Date = item.OccurrenceDate ?? item.Date;
        var _Length = series.EndDate.DayNumber - series.StartDate.DayNumber;

        _Form.StartDate = CalendarFormat.IsoDate(_Date);
        _Form.EndDate = CalendarFormat.IsoDate(_Date.AddDays(_Length));

        return _Form;
    }

    /// <summary>
    /// What is wrong with the form, or null when it can be sent. The API validates too; this only
    /// catches what a person would want told before a round trip.
    /// </summary>
    public string? Problem()
    {
        if (string.IsNullOrWhiteSpace(this.Title))
            return "Give it a name.";

        var _Start = CalendarFormat.ParseDate(this.StartDate);
        var _End = CalendarFormat.ParseDate(this.EndDate);

        if (_Start == null)
            return "Pick a start date.";

        if (_End != null && _End < _Start)
            return "It cannot end before it starts.";

        if (!this.IsAllDay)
        {
            var _StartTime = CalendarFormat.ParseTime(this.StartTime);
            var _EndTime = CalendarFormat.ParseTime(this.EndTime);

            if (_StartTime == null || _EndTime == null)
                return "Give it a start and an end time, or make it all day.";

            if ((_End ?? _Start) == _Start && _EndTime <= _StartTime)
                return "It cannot end before it starts.";
        }

        if (this.Frequency != CalendarRecurrenceFrequency.None && this.RepeatUntil.Length > 0)
        {
            var _Until = CalendarFormat.ParseDate(this.RepeatUntil);

            if (_Until != null && _Until < _Start)
                return "The repeat cannot end before the event starts.";
        }

        return null;
    }

    public CalendarEventWebAppRequest ToRequest(string timeZoneID)
    {
        var _Start = CalendarFormat.ParseDate(this.StartDate) ?? DateOnly.MinValue;
        var _End = CalendarFormat.ParseDate(this.EndDate) ?? _Start;
        var _Repeats = this.Frequency != CalendarRecurrenceFrequency.None;

        return new CalendarEventWebAppRequest()
        {
            DaysOfWeek = this.Frequency == CalendarRecurrenceFrequency.Weekly ? this.DaysOfWeek : 0,
            EndDate = _End < _Start ? _Start : _End,
            EndTime = this.IsAllDay ? null : CalendarFormat.ParseTime(this.EndTime),
            Frequency = this.Frequency,
            Interval = _Repeats ? Math.Clamp(this.Interval, 1, 99) : 1,
            IsAllDay = this.IsAllDay,
            Location = this.Location.Trim(),
            MemberUserIDs = [.. this.MemberUserIDs],
            Notes = this.Notes.Trim(),
            RepeatsOnWeekdayOfMonth = this.Frequency == CalendarRecurrenceFrequency.Monthly && this.RepeatsOnWeekdayOfMonth,
            RepeatUntil = _Repeats ? CalendarFormat.ParseDate(this.RepeatUntil) : null,
            StartDate = _Start,
            StartTime = this.IsAllDay ? null : CalendarFormat.ParseTime(this.StartTime),
            TimeZoneID = this.IsAllDay ? null : timeZoneID,
            Title = this.Title.Trim()
        };
    }

    #endregion Methods

}
