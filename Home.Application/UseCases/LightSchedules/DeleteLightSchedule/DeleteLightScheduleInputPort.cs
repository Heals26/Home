using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.LightSchedules.DeleteLightSchedule;

public record DeleteLightScheduleInputPort(long LightScheduleID)
    : IInputPort<IDeleteLightScheduleOutputPort>;
