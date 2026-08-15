using Home.Application.Services.Weather;

namespace Home.Application.UseCases.Weather.GetWeather;

public interface IGetWeatherOutputPort
{

    #region Methods

    /// <summary>
    /// The household has no coordinates yet. Distinct from a failure so the UI can point at
    /// Settings instead of showing an error.
    /// </summary>
    Task PresentLocationNotSetAsync(CancellationToken cancellationToken);

    Task PresentWeatherAsync(WeatherSnapshot weather, CancellationToken cancellationToken);
    Task PresentWeatherUnavailableAsync(CancellationToken cancellationToken);

    #endregion Methods

}
