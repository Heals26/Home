namespace Home.WebApi.Infrastructure.Weather;

internal static class WeatherValues
{

    #region Constants

    /// <summary>
    /// How long a good answer is reused. An always-on kitchen tablet plus a phone each would
    /// otherwise ask a free service on every page load; the weather does not move that fast.
    /// </summary>
    public const int CacheMinutes = 15;

    /// <summary>
    /// Three decimal places is roughly a hundred metres — enough that nudging the pin on the
    /// Settings map still hits the same cache entry.
    /// </summary>
    public const int CoordinateDecimalPlaces = 3;

    /// <summary>
    /// A failure is remembered too, but only briefly, so a forecaster that is down doesn't get a
    /// request from every device in the house while it recovers.
    /// </summary>
    public const int FailureCacheMinutes = 1;

    /// <summary>
    /// Today plus the two days after it — as much as fits a dashboard tile.
    /// </summary>
    public const int ForecastDays = 3;

    /// <summary>
    /// The Open-Meteo API root. Trailing slash matters — relative request URIs are resolved
    /// against it, and without it the "v1" segment is dropped.
    /// </summary>
    public const string OpenMeteoBaseUrl = "https://api.open-meteo.com/v1/";

    /// <summary>
    /// Short by HTTP standards, because a wall tablet showing a spinner is worse than one saying
    /// the weather is unavailable.
    /// </summary>
    public const int RequestTimeoutSeconds = 8;

    #endregion Constants

}
