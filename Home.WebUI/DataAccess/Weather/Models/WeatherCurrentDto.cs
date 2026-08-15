namespace Home.WebUI.DataAccess.Weather.Models;

public class WeatherCurrentDto
{

    #region Properties

    /// <summary>
    /// What it feels like, in degrees Celsius — the number people dress by.
    /// </summary>
    public double ApparentTemperatureCelsius { get; set; }

    /// <summary>
    /// The condition in plain words, e.g. "Partly cloudy".
    /// </summary>
    public string Condition { get; set; } = string.Empty;

    /// <summary>
    /// The <c>home-icon-{name}</c> suffix to draw, already resolved for day or night.
    /// </summary>
    public string IconName { get; set; } = string.Empty;

    /// <summary>
    /// True between sunrise and sunset at the household's coordinates.
    /// </summary>
    public bool IsDaytime { get; set; }

    /// <summary>
    /// Rain that has fallen in the current reporting interval, in millimetres.
    /// </summary>
    public double PrecipitationMillimetres { get; set; }

    public int RelativeHumidityPercentage { get; set; }

    /// <summary>
    /// When the API last asked the forecaster. Answers are cached server-side, so this can be a
    /// few minutes old.
    /// </summary>
    public DateTime RetrievedUTC { get; set; }

    public double TemperatureCelsius { get; set; }

    #endregion Properties

}
