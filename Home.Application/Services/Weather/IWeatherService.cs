namespace Home.Application.Services.Weather;

/// <summary>
/// The boundary to whatever actually reports the weather. Implemented in the outer layer so the
/// use cases never know which forecaster is on the other end.
/// </summary>
public interface IWeatherService
{

    #region Methods

    /// <summary>
    /// Conditions now and for the next few days at the given coordinates, or null if the
    /// forecaster could not be reached. Implementations must not throw for an unreachable
    /// forecaster — a dropped internet connection is a normal Tuesday, not an exception.
    /// Implementations are expected to cache, because a wall tablet asks far more often than
    /// the weather changes.
    /// </summary>
    Task<WeatherSnapshot?> GetForecastAsync(double latitude, double longitude, CancellationToken cancellationToken);

    #endregion Methods

}
