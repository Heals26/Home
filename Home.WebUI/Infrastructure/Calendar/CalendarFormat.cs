using Home.WebUI.DataAccess.Calendar.Models;
using System.Globalization;

namespace Home.WebUI.Infrastructure.Calendar;

/// <summary>
/// How the calendar writes dates and times, in one place so a chip, a sheet and the dashboard
/// never disagree about what "4pm" looks like. Weeks start on Monday, as the meal planner's do.
/// </summary>
public static class CalendarFormat
{

    #region Fields

    public static readonly string[] DayInitials = ["S", "M", "T", "W", "T", "F", "S"];

    #endregion Fields

    #region Methods

    public static string DayHeading(DateOnly date, DateOnly today)
    {
        if (date == today)
            return "Today";

        if (date == today.AddDays(1))
            return "Tomorrow";

        return date.ToString("dddd", CultureInfo.CurrentCulture);
    }

    public static string IsoDate(DateOnly date)
        => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static string IsoTime(TimeOnly time)
        => time.ToString("HH:mm", CultureInfo.InvariantCulture);

    public static string LongDate(DateOnly date)
        => date.ToString("dddd d MMMM", CultureInfo.CurrentCulture);

    public static string MonthLabel(DateOnly firstOfMonth)
        => firstOfMonth.ToString("MMMM yyyy", CultureInfo.CurrentCulture);

    public static DateOnly? ParseDate(string value)
        => DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var _Date) ? _Date : null;

    public static TimeOnly? ParseTime(string value)
        => TimeOnly.TryParse(value, CultureInfo.InvariantCulture, out var _Time) ? _Time : null;

    /// <summary>
    /// "All day", "4:00pm to 5:00pm", or "From 4:00pm" when an item runs on past this day.
    /// </summary>
    public static string Span(CalendarItemDto item)
    {
        if (item.StartsAt == null)
            return item.EndDate > item.StartDate ? $"All day, until {ShortDate(item.EndDate)}" : "All day";

        if (item.Kind is CalendarItemKind.Meal or CalendarItemKind.Task)
            return Time(item.StartsAt.Value);

        if (item.Date > item.StartDate && item.EndsAt == null)
            return "All day";

        if (item.EndsAt is not { } _EndsAt)
            return $"From {Time(item.StartsAt.Value)}";

        return item.Date > item.StartDate
            ? $"Until {Time(_EndsAt)}"
            : $"{Time(item.StartsAt.Value)} to {Time(_EndsAt)}";
    }

    public static DateOnly StartOfWeek(DateOnly date)
        => date.AddDays(-(((int)date.DayOfWeek + 6) % 7));

    public static string ShortDate(DateOnly date)
        => date.ToString("ddd d MMM", CultureInfo.CurrentCulture);

    /// <summary>
    /// "4pm" on the hour, "4:30pm" otherwise. Short, because it sits in a narrow column.
    /// </summary>
    public static string Time(TimeOnly time)
        => (time.Minute == 0 ? time.ToString("htt", CultureInfo.InvariantCulture) : time.ToString("h:mmtt", CultureInfo.InvariantCulture)).ToLowerInvariant();

    public static string WeekLabel(DateOnly weekStart)
    {
        var _End = weekStart.AddDays(6);

        return weekStart.Month == _End.Month
            ? $"{weekStart.Day} to {_End:d MMMM yyyy}"
            : $"{weekStart:d MMM} to {_End:d MMM yyyy}";
    }

    #endregion Methods

}
