// Home.WebApi has nullable disabled project-wide, but IWeatherService's contract is nullable-aware
// (a null snapshot means "forecaster unreachable"). Opting this file in keeps that meaning.
#nullable enable

using Home.Application.Services.Weather;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Text.Json;

namespace Home.WebApi.Infrastructure.Weather;

/// <summary>
/// Reads the forecast from Open-Meteo (https://open-meteo.com), which needs no key and no account.
/// Every call round-trips to the internet, so the forecaster being unreachable is an expected
/// outcome rather than an exception — the surface returns null instead of throwing.
/// </summary>
internal class OpenMeteoWeatherService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    ILogger<OpenMeteoWeatherService> logger,
    TimeProvider timeProvider) : IWeatherService
{

    #region Fields

    private static readonly JsonSerializerOptions s_JsonOptions = new() { PropertyNameCaseInsensitive = true };

    #endregion Fields

    #region Methods

    public async Task<WeatherSnapshot?> GetForecastAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken)
    {
        var _CacheKey = CacheKey(latitude, longitude);

        if (memoryCache.TryGetValue(_CacheKey, out WeatherSnapshot? _Cached))
            return _Cached;

        var _Snapshot = await this.FetchAsync(latitude, longitude, cancellationToken);

        _ = memoryCache.Set(_CacheKey, _Snapshot, TimeSpan.FromMinutes(
            _Snapshot == null ? WeatherValues.FailureCacheMinutes : WeatherValues.CacheMinutes));

        return _Snapshot;
    }

    private async Task<WeatherSnapshot?> FetchAsync(
        double latitude,
        double longitude,
        CancellationToken cancellationToken)
    {
        try
        {
            using var _Response = await httpClient.GetAsync(BuildRequestUri(latitude, longitude), cancellationToken);

            if (!_Response.IsSuccessStatusCode)
            {
                logger.LogWarning("Open-Meteo returned {StatusCode} fetching the forecast.", _Response.StatusCode);
                return null;
            }

            var _Payload = await _Response.Content.ReadAsStringAsync(cancellationToken);
            var _Forecast = JsonSerializer.Deserialize<OpenMeteoForecast>(_Payload, s_JsonOptions);

            return _Forecast?.Current == null ? null : this.ToSnapshot(_Forecast);
        }
        catch (Exception _Exception) when (_Exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            logger.LogWarning(_Exception, "Could not reach Open-Meteo to fetch the forecast.");
            return null;
        }
    }

    private WeatherSnapshot ToSnapshot(OpenMeteoForecast forecast)
        => new(
            WeatherConditionMap.FromWmoCode(forecast.Current.WeatherCode),
            forecast.Current.Temperature,
            forecast.Current.ApparentTemperature,
            forecast.Current.RelativeHumidity,
            forecast.Current.Precipitation,
            forecast.Current.IsDay == 1,
            timeProvider.GetUtcNow().UtcDateTime,
            ToForecast(forecast.Daily));

    /// <summary>
    /// "timezone=auto" resolves the days against the coordinates rather than UTC, so a Brisbane
    /// household's "today" is the day it is in Brisbane.
    /// </summary>
    private static string BuildRequestUri(double latitude, double longitude)
        => "forecast"
            + $"?latitude={Format(latitude)}"
            + $"&longitude={Format(longitude)}"
            + "&current=temperature_2m,apparent_temperature,relative_humidity_2m,precipitation,weather_code,is_day"
            + "&daily=temperature_2m_max,temperature_2m_min,precipitation_probability_max,weather_code"
            + "&timezone=auto"
            + $"&forecast_days={WeatherValues.ForecastDays}";

    private static string CacheKey(double latitude, double longitude)
        => $"weather:{Format(Math.Round(latitude, WeatherValues.CoordinateDecimalPlaces))}"
            + $":{Format(Math.Round(longitude, WeatherValues.CoordinateDecimalPlaces))}";

    // Invariant culture matters here: a machine set to a comma decimal separator would otherwise
    // send "latitude=-27,47" and Open-Meteo would reject the whole request.
    private static string Format(double value)
        => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// The daily block arrives as parallel arrays. Any day whose date will not parse is dropped
    /// rather than guessed at.
    /// </summary>
    private static IReadOnlyList<WeatherDayForecast> ToForecast(OpenMeteoDaily? daily)
    {
        if (daily?.Time == null)
            return [];

        var _Days = new List<WeatherDayForecast>(daily.Time.Count);

        for (var _Index = 0; _Index < daily.Time.Count; _Index++)
        {
            if (!DateTime.TryParse(daily.Time[_Index], CultureInfo.InvariantCulture, DateTimeStyles.None, out var _Date))
                continue;

            _Days.Add(new WeatherDayForecast(
                _Date.Date,
                WeatherConditionMap.FromWmoCode(ValueAt(daily.WeatherCode, _Index)),
                ValueAt(daily.TemperatureMaximum, _Index),
                ValueAt(daily.TemperatureMinimum, _Index),
                ValueAt(daily.PrecipitationProbabilityMaximum, _Index) ?? 0));
        }

        return _Days;
    }

    private static T? ValueAt<T>(List<T>? values, int index)
        => values != null && index < values.Count ? values[index] : default;

    #endregion Methods

}
