namespace Home.WebUI.Infrastructure.Services.Time;

/// <summary>
/// The viewer's own clock. Blazor Server runs on the server, so <c>DateTime.ToLocalTime()</c> and
/// <c>TimeProvider.GetLocalNow()</c> answer in the server's zone, which is wrong the day this is
/// hosted somewhere else. This reads the browser's zone once per circuit and converts through it.
/// </summary>
public interface IViewerClock
{

    #region Methods

    /// <summary>
    /// The browser's IANA zone name, for example "Australia/Brisbane". Falls back to the server's
    /// zone if the browser cannot say.
    /// </summary>
    Task<string> GetTimeZoneIDAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Today in the viewer's zone.
    /// </summary>
    Task<DateOnly> TodayAsync(CancellationToken cancellationToken);

    /// <summary>
    /// A UTC instant as the viewer sees it. Only valid after one of the async members has run on
    /// this circuit; before that it answers in UTC.
    /// </summary>
    DateTime ToLocal(DateTime utc);

    #endregion Methods

}
