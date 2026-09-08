namespace Home.Domain.Entities;

/// <summary>
/// A read-only iCalendar feed the household already keeps elsewhere (Google, Apple, Outlook).
/// Home fetches it on a timer and stores the expanded occurrences as its own read-only events.
/// </summary>
public class CalendarSubscription
{

    #region Properties

    public long CalendarSubscriptionID { get; set; }

    /// <summary>
    /// Why the last fetch failed, or null when it succeeded. The last good occurrences are kept
    /// either way, because a stale calendar beats a blank one.
    /// </summary>
    public string? LastError { get; set; }

    public DateTime? LastFetchedUTC { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The feed's secret address. Treated like a token: shown back only as its host.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    public ICollection<CalendarEvent> Events { get; set; } = [];

    public Household Household { get; set; } = null!;

    #endregion Properties

}
