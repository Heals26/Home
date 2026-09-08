namespace Home.Application.Infrastructure.Calendar;

public static class CalendarValues
{

    #region Fields

    /// <summary>
    /// The widest window one calendar read may ask for. A six-week month grid is 42 days.
    /// </summary>
    public static readonly int MaximumWindowDays = 62;

    /// <summary>
    /// A repeat that has produced this many occurrences has run away; nothing on a kitchen
    /// tablet legitimately needs more inside one window.
    /// </summary>
    public static readonly int MaximumOccurrencesPerSeries = 1000;

    /// <summary>
    /// How far back a subscription's occurrences are kept.
    /// </summary>
    public static readonly int SubscriptionWindowDaysBack = 31;

    /// <summary>
    /// How far forward a subscription's occurrences are expanded.
    /// </summary>
    public static readonly int SubscriptionWindowDaysForward = 366;

    public static readonly string UtcTimeZoneID = "UTC";

    #endregion Fields

}
