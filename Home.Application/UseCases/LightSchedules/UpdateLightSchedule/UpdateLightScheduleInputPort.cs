using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;
using Home.Domain.Enumerations;

namespace Home.Application.UseCases.LightSchedules.UpdateLightSchedule;

public record UpdateLightScheduleInputPort(
    long LightScheduleID,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<bool> IsEnabled,
    PropertyChangeTracker<TimeSpan> TimeOfDay,
    PropertyChangeTracker<int> DaysOfWeek,
    PropertyChangeTracker<LightScheduleCondition> Condition)
    : IInputPort<IUpdateLightScheduleOutputPort>;
