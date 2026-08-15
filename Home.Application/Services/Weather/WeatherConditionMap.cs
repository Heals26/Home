namespace Home.Application.Services.Weather;

/// <summary>
/// WMO weather interpretation codes (the standard Open-Meteo and most other forecasters publish)
/// collapsed to <see cref="WeatherCondition"/>, plus how each condition reads and draws. This is
/// transient external data rather than domain state, which is why it lives beside the service
/// boundary and not in Home.Domain.
/// </summary>
public static class WeatherConditionMap
{

    #region Methods

    public static WeatherCondition FromWmoCode(int wmoCode)
        => wmoCode switch
        {
            0 => WeatherCondition.Clear,
            1 or 2 => WeatherCondition.PartlyCloudy,
            3 => WeatherCondition.Cloudy,
            45 or 48 => WeatherCondition.Fog,
            51 or 53 or 55 or 56 or 57 => WeatherCondition.Drizzle,
            61 or 63 or 66 or 67 => WeatherCondition.Rain,
            65 => WeatherCondition.HeavyRain,
            71 or 73 or 75 or 77 or 85 or 86 => WeatherCondition.Snow,
            80 or 81 => WeatherCondition.Showers,
            82 => WeatherCondition.HeavyRain,
            95 or 96 or 99 => WeatherCondition.Thunderstorm,
            _ => WeatherCondition.Unknown
        };

    public static string GetDescription(WeatherCondition condition)
        => condition switch
        {
            WeatherCondition.Clear => "Clear",
            WeatherCondition.PartlyCloudy => "Partly cloudy",
            WeatherCondition.Cloudy => "Cloudy",
            WeatherCondition.Fog => "Fog",
            WeatherCondition.Drizzle => "Drizzle",
            WeatherCondition.Rain => "Rain",
            WeatherCondition.HeavyRain => "Heavy rain",
            WeatherCondition.Showers => "Showers",
            WeatherCondition.Snow => "Snow",
            WeatherCondition.Thunderstorm => "Thunderstorms",
            _ => "Unknown"
        };

    /// <summary>
    /// The <c>home-icon-{name}</c> suffix the UI draws for this condition. Clear and broken cloud
    /// read differently after dark, so the sun swaps for a moon.
    /// </summary>
    public static string GetIconName(WeatherCondition condition, bool isDaytime)
        => condition switch
        {
            WeatherCondition.Clear => isDaytime ? "weather-sun" : "weather-moon",
            WeatherCondition.PartlyCloudy => isDaytime ? "weather-cloud-sun" : "weather-cloud-moon",
            WeatherCondition.Cloudy => "weather-cloud",
            WeatherCondition.Fog => "weather-fog",
            WeatherCondition.Drizzle => "weather-drizzle",
            WeatherCondition.Rain => "weather-rain",
            WeatherCondition.HeavyRain => "weather-rain-heavy",
            WeatherCondition.Showers => "weather-showers",
            WeatherCondition.Snow => "weather-snow",
            WeatherCondition.Thunderstorm => "weather-storm",
            _ => "weather-cloud"
        };

    #endregion Methods

}
