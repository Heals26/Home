using CleanArchitecture.Mediator;
using Home.Application.Services.Security;
using Home.Application.Services.Weather;

namespace Home.Application.UseCases.Weather.GetWeather;

/// <summary>
/// The dashboard's weather. The coordinates are read off the caller's own household row rather
/// than taken from the request, so there is no ID a caller could swap to read where another
/// family lives.
/// </summary>
internal class GetWeatherInteractor : IInteractor<GetWeatherInputPort, IGetWeatherOutputPort>
{

    #region Methods

    public async Task HandleAsync(
        GetWeatherInputPort inputPort,
        IGetWeatherOutputPort outputPort,
        ServiceFactory serviceFactory,
        CancellationToken cancellationToken)
    {
        var _AuthorisationService = serviceFactory.GetService<IAuthorisationService>();
        var _WeatherService = serviceFactory.GetService<IWeatherService>();

        var _Household = _AuthorisationService.GetHousehold();

        if (_Household.Latitude == null || _Household.Longitude == null)
            await outputPort.PresentLocationNotSetAsync(cancellationToken);
        else
        {
            var _Weather = await _WeatherService.GetForecastAsync(
                _Household.Latitude.Value,
                _Household.Longitude.Value,
                cancellationToken);

            if (_Weather == null)
                await outputPort.PresentWeatherUnavailableAsync(cancellationToken);
            else
                await outputPort.PresentWeatherAsync(_Weather, cancellationToken);
        }
    }

    #endregion Methods

}
