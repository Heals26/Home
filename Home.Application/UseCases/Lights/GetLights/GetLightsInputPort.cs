using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Lights.GetLights;

public record GetLightsInputPort() : IInputPort<IGetLightsOutputPort>;
