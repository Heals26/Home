using Home.Domain.Enumerations;

namespace Home.Domain.Entities;

/// <summary>
/// Fires a saved scene on chosen days — at a fixed time of day, or relative to sunrise or
/// sunset computed from the household's stored latitude and longitude.
/// </summary>
public class LightSchedule
{

    #region Properties

    public long LightScheduleID { get; set; }

    /// <summary>
    /// Bitmask of <see cref="DayOfWeek"/> values — bit 0 is Sunday, matching the enum. Zero means
    /// the schedule never fires.
    /// </summary>
    public int DaysOfWeek { get; set; }

    public bool IsEnabled { get; set; }

    /// <summary>
    /// The last time this schedule actually fired, used to stop it firing twice in one day.
    /// </summary>
    public DateTime? LastRunUTC { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Minutes relative to the sun event when <see cref="Trigger"/> is sunrise or sunset —
    /// negative fires before it, positive after. Ignored for fixed-time schedules.
    /// </summary>
    public int OffsetMinutes { get; set; }

    /// <summary>
    /// Local time of day to fire at when <see cref="Trigger"/> is a fixed time. Stored as local
    /// rather than UTC so a schedule set for 7pm stays at 7pm across daylight saving.
    /// </summary>
    public TimeSpan TimeOfDay { get; set; }

    /// <summary>
    /// What starts the schedule. Sun triggers only fire when the household has a location set.
    /// </summary>
    public LightScheduleTrigger Trigger { get; set; }

    /// <summary>
    /// A schedule belongs to its scene, and the scene belongs to a household — there is
    /// deliberately no direct Household link. Two routes to the same household would give SQL
    /// Server two cascade paths to this table, which it rejects.
    /// </summary>
    public LightScene Scene { get; set; } = null!;

    #endregion Properties

}
