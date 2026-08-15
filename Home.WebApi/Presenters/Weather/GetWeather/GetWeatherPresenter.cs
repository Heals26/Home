using AutoMapper;
using Home.Application.Services.Weather;
using Home.Application.UseCases.Weather.GetWeather;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.Weather.GetWeather;
using Home.WebApi.UseCases.Weather.Models;

namespace Home.WebApi.Presenters.Weather.GetWeather;

public class GetWeatherPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetWeatherOutputPort
{

    #region Methods

    Task IGetWeatherOutputPort.PresentLocationNotSetAsync(CancellationToken cancellationToken)
        => this.OkAsync(new GetWeatherApiResponse() { HasLocation = false }, cancellationToken);

    Task IGetWeatherOutputPort.PresentWeatherAsync(WeatherSnapshot weather, CancellationToken cancellationToken)
        => this.OkAsync(new GetWeatherApiResponse()
        {
            HasLocation = true,
            Current = new WeatherCurrentDto()
            {
                ApparentTemperatureCelsius = weather.ApparentTemperatureCelsius,
                Condition = WeatherConditionMap.GetDescription(weather.Condition),
                IconName = WeatherConditionMap.GetIconName(weather.Condition, weather.IsDaytime),
                IsDaytime = weather.IsDaytime,
                PrecipitationMillimetres = weather.PrecipitationMillimetres,
                RelativeHumidityPercentage = weather.RelativeHumidityPercentage,
                RetrievedUTC = weather.RetrievedUTC,
                TemperatureCelsius = weather.TemperatureCelsius
            },
            Forecast = [.. weather.Forecast.Select(d => new WeatherDayDto()
            {
                Condition = WeatherConditionMap.GetDescription(d.Condition),
                Date = d.Date,
                IconName = WeatherConditionMap.GetIconName(d.Condition, isDaytime: true),
                MaximumTemperatureCelsius = d.MaximumTemperatureCelsius,
                MinimumTemperatureCelsius = d.MinimumTemperatureCelsius,
                PrecipitationProbabilityPercentage = d.PrecipitationProbabilityPercentage
            })]
        }, cancellationToken);

    Task IGetWeatherOutputPort.PresentWeatherUnavailableAsync(CancellationToken cancellationToken)
        => this.ServiceUnavailableAsync("The weather service could not be reached", cancellationToken);

    #endregion Methods

}
