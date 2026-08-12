using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.LightSchedules.CreateLightSchedule;

public record CreateLightScheduleInputPort(
    string Name,
    long LightSceneID,
    TimeSpan TimeOfDay,
    int DaysOfWeek)
    : IInputPort<ICreateLightScheduleOutputPort>;
