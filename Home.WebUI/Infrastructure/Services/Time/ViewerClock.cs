using Microsoft.JSInterop;

namespace Home.WebUI.Infrastructure.Services.Time;

/// <summary>
/// Scoped per circuit, so the zone is asked for once and every page on that tablet shares the
/// answer. The JS call fails while there is no circuit (a disconnected tab) or when the browser
/// gives nothing useful; both fall back to the server's zone rather than throwing, because a
/// calendar that will not open is worse than one an hour out.
/// </summary>
public class ViewerClock(IJSRuntime jsRuntime, TimeProvider timeProvider) : IViewerClock
{

    #region Fields

    private TimeZoneInfo? m_Zone;

    #endregion Fields

    #region Methods

    public async Task<string> GetTimeZoneIDAsync(CancellationToken cancellationToken)
        => (await this.ResolveAsync(cancellationToken)).Id;

    public DateTime ToLocal(DateTime utc)
        => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), this.m_Zone ?? TimeZoneInfo.Utc);

    public async Task<DateOnly> TodayAsync(CancellationToken cancellationToken)
    {
        var _Zone = await this.ResolveAsync(cancellationToken);

        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(timeProvider.GetUtcNow().UtcDateTime, _Zone));
    }

    private async Task<TimeZoneInfo> ResolveAsync(CancellationToken cancellationToken)
    {
        if (this.m_Zone != null)
            return this.m_Zone;

        string? _ID = null;

        try
        {
            _ID = await jsRuntime.InvokeAsync<string>("homeClock.timeZone", cancellationToken);
        }
        catch (Exception _Exception) when (_Exception is JSException or InvalidOperationException or TaskCanceledException or JSDisconnectedException)
        {
            // No circuit yet, or the browser would not say. The fallback below covers it.
        }

        this.m_Zone = !string.IsNullOrWhiteSpace(_ID) && TimeZoneInfo.TryFindSystemTimeZoneById(_ID, out var _Zone)
            ? _Zone
            : ServerZone();

        return this.m_Zone;
    }

    /// <summary>
    /// The server's zone under its IANA name, because that is the form the API expects. Windows
    /// names convert; anything that will not falls back to UTC.
    /// </summary>
    private static TimeZoneInfo ServerZone()
    {
        var _Local = TimeZoneInfo.Local;

        if (_Local.HasIanaId)
            return _Local;

        return TimeZoneInfo.TryConvertWindowsIdToIanaId(_Local.Id, out var _IanaID) && TimeZoneInfo.TryFindSystemTimeZoneById(_IanaID, out var _Zone)
            ? _Zone
            : TimeZoneInfo.Utc;
    }

    #endregion Methods

}
