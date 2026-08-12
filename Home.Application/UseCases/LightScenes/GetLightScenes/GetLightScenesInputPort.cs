using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.LightScenes.GetLightScenes;

public record GetLightScenesInputPort() : IInputPort<IGetLightScenesOutputPort>;
