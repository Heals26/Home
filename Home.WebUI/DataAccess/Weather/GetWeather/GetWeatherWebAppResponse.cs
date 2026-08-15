using Home.WebUI.DataAccess.Weather.Models;

namespace Home.WebUI.DataAccess.Weather.GetWeather;

public class GetWeatherWebAppResponse
{

    #region Properties

    /// <summary>
    /// Conditions now. Null when <see cref="HasLocation"/> is false.
    /// </summary>
    public WeatherCurrentDto? Current { get; set; }

    /// <summary>
    /// Today first, then the days after it. Empty when <see cref="HasLocation"/> is false.
    /// </summary>
    public List<WeatherDayDto> Forecast { get; set; } = [];

    /// <summary>
    /// False when the household has not set its coordinates. Show a prompt to set them in
    /// Settings rather than an error — nothing has gone wrong.
    /// </summary>
    public bool HasLocation { get; set; }

    #endregion Properties

}
