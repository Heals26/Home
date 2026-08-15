using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Weather.GetWeather;

public record GetWeatherInputPort() : IInputPort<IGetWeatherOutputPort>;
