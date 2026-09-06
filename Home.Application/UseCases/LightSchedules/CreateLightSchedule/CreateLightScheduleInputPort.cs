using CleanArchitecture.Mediator;
using Home.Domain.Enumerations;

namespace Home.Application.UseCases.LightSchedules.CreateLightSchedule;

public record CreateLightScheduleInputPort(
    string Name,
    long LightSceneID,
    LightScheduleTrigger Trigger,
    LightScheduleCondition Condition,
    TimeSpan TimeOfDay,
    int OffsetMinutes,
    int DaysOfWeek)
    : IInputPort<ICreateLightScheduleOutputPort>;
