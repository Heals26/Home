using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Lights.SyncAllLights;

public record SyncAllLightsInputPort() : IInputPort<ISyncAllLightsOutputPort>;
