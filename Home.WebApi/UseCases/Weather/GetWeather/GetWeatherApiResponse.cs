using Home.WebApi.UseCases.Weather.Models;

namespace Home.WebApi.UseCases.Weather.GetWeather;

public class GetWeatherApiResponse
{

    #region Properties

    /// <summary>
    /// Conditions now. Null when <see cref="HasLocation"/> is false.
    /// </summary>
    public WeatherCurrentDto Current { get; set; }

    /// <summary>
    /// Today first, then the days after it. Empty when <see cref="HasLocation"/> is false.
    /// </summary>
    public List<WeatherDayDto> Forecast { get; set; } = [];

    /// <summary>
    /// False when the household has not set its coordinates, so the UI can ask for them rather
    /// than reporting a failure.
    /// </summary>
    public bool HasLocation { get; set; }

    #endregion Properties

}
