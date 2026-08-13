using Home.WebUI.Infrastructure.ChangeTrackers;

namespace Home.WebUI.DataAccess.LightSchedules.UpdateLightSchedule;

/// <summary>
/// Omit a property to leave it alone. Sending only IsEnabled is how the UI toggles a schedule.
/// </summary>
public class UpdateLightScheduleWebAppRequest
{

    #region Properties

    /// <summary>
    /// Bitmask of days — bit 0 is Sunday.
    /// </summary>
    public PropertyChangeTracker<int> DaysOfWeek { get; set; }

    /// <summary>
    /// Whether the schedule fires at all.
    /// </summary>
    public PropertyChangeTracker<bool> IsEnabled { get; set; }

    /// <summary>
    /// The schedule's name.
    /// </summary>
    public PropertyChangeTracker<string> Name { get; set; }

    /// <summary>
    /// Local time of day the schedule fires at.
    /// </summary>
    public PropertyChangeTracker<TimeSpan> TimeOfDay { get; set; }

    #endregion Properties

}
