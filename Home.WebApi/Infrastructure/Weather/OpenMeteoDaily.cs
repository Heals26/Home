using System.Text.Json.Serialization;

namespace Home.WebApi.Infrastructure.Weather;

/// <summary>
/// The wire shape of the "daily" block of an Open-Meteo forecast response. It arrives as parallel
/// arrays rather than a list of days, so every list is indexed against
/// <see cref="Time"/>.
/// </summary>
internal class OpenMeteoDaily
{

    #region Properties

    /// <summary>
    /// Nullable because some Open-Meteo models omit a probability for days they cannot call.
    /// </summary>
    [JsonPropertyName("precipitation_probability_max")]
    public List<int?> PrecipitationProbabilityMaximum { get; set; }

    [JsonPropertyName("temperature_2m_max")]
    public List<double> TemperatureMaximum { get; set; }

    [JsonPropertyName("temperature_2m_min")]
    public List<double> TemperatureMinimum { get; set; }

    /// <summary>
    /// The local calendar days, ISO formatted, e.g. "2026-08-15".
    /// </summary>
    [JsonPropertyName("time")]
    public List<string> Time { get; set; }

    [JsonPropertyName("weather_code")]
    public List<int> WeatherCode { get; set; }

    #endregion Properties

}
