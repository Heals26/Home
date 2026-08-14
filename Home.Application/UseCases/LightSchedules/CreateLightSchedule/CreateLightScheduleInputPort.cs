using CleanArchitecture.Mediator;
using Home.Domain.Enumerations;

namespace Home.Application.UseCases.LightSchedules.CreateLightSchedule;

public record CreateLightScheduleInputPort(
    string Name,
    long LightSceneID,
    LightScheduleTrigger Trigger,
    TimeSpan TimeOfDay,
    int OffsetMinutes,
    int DaysOfWeek)
    : IInputPort<ICreateLightScheduleOutputPort>;
