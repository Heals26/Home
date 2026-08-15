namespace Home.WebApi.UseCases.Weather.Models;

public class WeatherDayDto
{

    #region Properties

    /// <summary>
    /// The day's condition in plain words, e.g. "Showers".
    /// </summary>
    public string Condition { get; set; }

    /// <summary>
    /// The local calendar day at midnight.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// The <c>home-icon-{name}</c> suffix to draw. Always the daytime variant.
    /// </summary>
    public string IconName { get; set; }

    public double MaximumTemperatureCelsius { get; set; }

    public double MinimumTemperatureCelsius { get; set; }

    /// <summary>
    /// The highest chance of rain across the day, 0 to 100.
    /// </summary>
    public int PrecipitationProbabilityPercentage { get; set; }

    #endregion Properties

}
