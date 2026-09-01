using Home.WebUI.Components.Shared.Inputs;
using Home.WebUI.DataAccess.Activities.Models;
using System.Globalization;

namespace Home.WebUI.Components.Pages.Activities;

/// <summary>
/// The shaping the board, the week, the day and the card detail all need — kept in one place so
/// the views cannot drift apart on how a due time reads or how a day is ordered.
/// </summary>
public static class ActivityBoardLogic
{

    #region Fields

    private const string FallbackColour = "#a8a29e";

    #endregion Fields

    #region Methods

    public static string DescribeDate(DateTime date)
        => date.ToString("d MMM", CultureInfo.CurrentCulture);

    public static string DescribeDay(DateTime day)
        => day.ToString("ddd", CultureInfo.CurrentCulture);

    public static string DescribeLongDay(DateTime day)
        => day.ToString("dddd d MMMM", CultureInfo.CurrentCulture);

    // Built from the parts rather than a DateTime so no part of this touches the clock.
    public static string DescribeTime(TimeSpan time)
    {
        var _Hour = time.Hours % 12 == 0 ? 12 : time.Hours % 12;

        return $"{_Hour}:{time.Minutes:00} {(time.Hours < 12 ? "am" : "pm")}";
    }

    public static List<ActivitySummaryDto> ForDay(IEnumerable<ActivitySummaryDto> activities, DateTime day)
        => OrderForDay(activities.Where(a => a.DueDateUTC.HasValue && a.DueDateUTC.Value.Date == day.Date));

    public static string FormatTime(TimeSpan? time)
        => time.HasValue ? time.Value.ToString(@"hh\:mm", CultureInfo.InvariantCulture) : string.Empty;

    public static string Initials(string? name)
    {
        var _Parts = (name ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return _Parts.Length switch
        {
            0 => "?",
            1 => _Parts[0][..1].ToUpperInvariant(),
            _ => $"{char.ToUpperInvariant(_Parts[0][0])}{char.ToUpperInvariant(_Parts[^1][0])}"
        };
    }

    /// <summary>
    /// Untimed first, because "sometime today" is the normal case in a house and burying it under
    /// the timed items is how it gets missed.
    /// </summary>
    public static List<ActivitySummaryDto> OrderForDay(IEnumerable<ActivitySummaryDto> activities)
        => [.. activities
            .OrderBy(a => a.DueTime.HasValue)
            .ThenBy(a => a.DueTime ?? TimeSpan.Zero)
            .ThenBy(a => a.Title)];

    public static TimeSpan? ParseTime(string? value)
        => TimeSpan.TryParse(value, CultureInfo.InvariantCulture, out var _Parsed) ? _Parsed : null;


    /// <summary>
    /// A label's colour is chosen by the family at runtime, so it can only ever go into an inline
    /// style — and only after it has been proved to be #RRGGBB.
    /// </summary>
    public static string SafeColour(string? colour)
        => HomeColourPicker.IsValidColour(colour) ? colour! : FallbackColour;

    /// <summary>
    /// The Monday of the week the day falls in — a family week starts on a Monday regardless of
    /// what the machine's culture says.
    /// </summary>
    public static DateTime StartOfWeek(DateTime day)
        => day.Date.AddDays(-(((int)day.DayOfWeek + 6) % 7));

    public static List<ActivitySummaryDto> WithoutDueDate(IEnumerable<ActivitySummaryDto> activities)
        => [.. activities.Where(a => !a.DueDateUTC.HasValue).OrderBy(a => a.Title)];

    #endregion Methods

}
