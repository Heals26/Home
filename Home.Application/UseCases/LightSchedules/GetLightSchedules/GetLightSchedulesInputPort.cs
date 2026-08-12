using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.LightSchedules.GetLightSchedules;

public record GetLightSchedulesInputPort() : IInputPort<IGetLightSchedulesOutputPort>;
