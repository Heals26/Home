using System.Globalization;

namespace Home.WebUI.Infrastructure.Time;

/// <summary>
/// Where a week starts, in one place. The board, the calendar and the meal plan each worked it out
/// for themselves, and a house that disagrees with itself about which days are "this week" is worse
/// than either answer.
/// </summary>
public static class WeekLogic
{

    #region Fields

    /// <summary>
    /// Sunday (20 Sep 2026), whatever the machine's culture says.
    /// </summary>
    public const DayOfWeek FirstDay = DayOfWeek.Sunday;

    #endregion Fields

    #region Properties

    /// <summary>
    /// The seven days in the order a week reads here, for a heading row or a day picker.
    /// </summary>
    public static IReadOnlyList<DayOfWeek> Order { get; } =
        [.. Enumerable.Range(0, 7).Select(i => (DayOfWeek)(((int)FirstDay + i) % 7))];

    /// <summary>
    /// The same order, as the short names a column heading shows.
    /// </summary>
    public static IReadOnlyList<string> ShortNames { get; } =
        [.. Order.Select(d => CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedDayName(d))];

    #endregion Properties

    #region Methods

    private static int DaysInto(DayOfWeek day)
        => ((int)day - (int)FirstDay + 7) % 7;

    public static DateOnly StartOfWeek(DateOnly date)
        => date.AddDays(-DaysInto(date.DayOfWeek));

    public static DateTime StartOfWeek(DateTime date)
        => date.Date.AddDays(-DaysInto(date.DayOfWeek));

    #endregion Methods

}
