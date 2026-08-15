using System.Text.Json.Serialization;

namespace Home.WebApi.Infrastructure.Weather;

/// <summary>
/// The wire shape of the "current" block of an Open-Meteo forecast response. Kept internal and
/// separate from <see cref="Home.Application.Services.Weather.WeatherSnapshot"/> so a change at
/// Open-Meteo's end doesn't leak into the use cases.
/// </summary>
internal class OpenMeteoCurrent
{

    #region Properties

    [JsonPropertyName("apparent_temperature")]
    public double ApparentTemperature { get; set; }

    /// <summary>1 during daylight, 0 after dark. Open-Meteo sends a number, not a boolean.</summary>
    [JsonPropertyName("is_day")]
    public int IsDay { get; set; }

    [JsonPropertyName("precipitation")]
    public double Precipitation { get; set; }

    [JsonPropertyName("relative_humidity_2m")]
    public int RelativeHumidity { get; set; }

    [JsonPropertyName("temperature_2m")]
    public double Temperature { get; set; }

    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; set; }

    #endregion Properties

}
