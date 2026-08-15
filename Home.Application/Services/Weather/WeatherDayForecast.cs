namespace Home.Application.Services.Weather;

/// <summary>
/// One forecast day. The date is the household's local calendar day at midnight, matching how
/// dated rows are stored elsewhere in Home.
/// </summary>
public record WeatherDayForecast(
    DateTime Date,
    WeatherCondition Condition,
    double MaximumTemperatureCelsius,
    double MinimumTemperatureCelsius,
    int PrecipitationProbabilityPercentage);
