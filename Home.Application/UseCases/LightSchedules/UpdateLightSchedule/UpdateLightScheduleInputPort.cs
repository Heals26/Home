using CleanArchitecture.Mediator;
using Home.Application.Infrastructure.ChangeTrackers;

namespace Home.Application.UseCases.LightSchedules.UpdateLightSchedule;

public record UpdateLightScheduleInputPort(
    long LightScheduleID,
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<bool> IsEnabled,
    PropertyChangeTracker<TimeSpan> TimeOfDay,
    PropertyChangeTracker<int> DaysOfWeek)
    : IInputPort<IUpdateLightScheduleOutputPort>;
