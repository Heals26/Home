using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Lights.SyncLights;

public record SyncLightsInputPort() : IInputPort<ISyncLightsOutputPort>;
