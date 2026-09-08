namespace Home.Application.Services.Calendar;

/// <summary>
/// The boundary to whatever fetches and expands an iCalendar feed. Implemented in the outer layer
/// so the use cases never learn which parser is on the other end.
/// </summary>
public interface ICalendarFeedService
{

    #region Methods

    /// <summary>
    /// Every occurrence in the feed that starts inside the window, or null when the feed could not
    /// be fetched or read. Implementations must not throw for an unreachable or malformed feed: a
    /// dropped connection or a provider hiccup is a normal Tuesday, not an exception.
    /// </summary>
    Task<CalendarFeed?> ReadAsync(string url, DateTime windowStartUTC, DateTime windowEndUTC, CancellationToken cancellationToken);

    #endregion Methods

}
