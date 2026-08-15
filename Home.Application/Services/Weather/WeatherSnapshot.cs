namespace Home.Application.Services.Weather;

/// <summary>
/// Conditions now plus the next few days, as reported by the forecaster. Transient external data —
/// nothing here is persisted, and the retrieval time is how stale it is allowed to look given the
/// service caches its answers.
/// </summary>
public record WeatherSnapshot(
    WeatherCondition Condition,
    double TemperatureCelsius,
    double ApparentTemperatureCelsius,
    int RelativeHumidityPercentage,
    double PrecipitationMillimetres,
    bool IsDaytime,
    DateTime RetrievedUTC,
    IReadOnlyList<WeatherDayForecast> Forecast);
