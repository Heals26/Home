namespace Home.Application.Services.Weather;

/// <summary>
/// The handful of conditions a family reads off a kitchen tablet. Deliberately far coarser than
/// the WMO code list it is collapsed from — nobody plans their day around the difference between
/// light and moderate drizzle.
/// </summary>
public enum WeatherCondition
{

    /// <summary>The forecaster reported a code Home does not recognise.</summary>
    Unknown = 0,

    /// <summary>Cloudless, or near enough.</summary>
    Clear = 1,

    /// <summary>Broken cloud with sun or moon through it.</summary>
    PartlyCloudy = 2,

    /// <summary>Overcast.</summary>
    Cloudy = 3,

    /// <summary>Fog or rime fog.</summary>
    Fog = 4,

    /// <summary>Drizzle, freezing or otherwise.</summary>
    Drizzle = 5,

    /// <summary>Steady rain.</summary>
    Rain = 6,

    /// <summary>Heavy rain — worth taking the washing in for.</summary>
    HeavyRain = 7,

    /// <summary>Passing showers rather than steady rain.</summary>
    Showers = 8,

    /// <summary>Snow, snow grains or snow showers.</summary>
    Snow = 9,

    /// <summary>Thunderstorms, with or without hail.</summary>
    Thunderstorm = 10,

}
