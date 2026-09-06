using Home.Application.Infrastructure.ChangeTrackers;
using Home.Domain.Enumerations;

namespace Home.WebApi.UseCases.LightSchedules.UpdateLightSchedule;

/// <summary>
/// Omit a property to leave it alone. Sending only IsEnabled is how the UI toggles a schedule.
/// </summary>
public record UpdateLightScheduleApiRequest(
    PropertyChangeTracker<string> Name,
    PropertyChangeTracker<bool> IsEnabled,
    PropertyChangeTracker<TimeSpan> TimeOfDay,
    PropertyChangeTracker<int> DaysOfWeek,
    PropertyChangeTracker<LightScheduleCondition> Condition);
