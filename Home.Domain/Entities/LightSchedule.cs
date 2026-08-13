namespace Home.Domain.Entities;

/// <summary>
/// Fires a saved scene at a time of day on chosen days. Sunrise and sunset triggers are
/// deliberately absent: they need the household's latitude and longitude, which Home does not
/// collect yet, and a half-working trigger is worse than an obviously missing one.
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
    /// Local time of day to fire at. Stored as local rather than UTC so a schedule set for 7pm
    /// stays at 7pm across daylight saving.
    /// </summary>
    public TimeSpan TimeOfDay { get; set; }

    /// <summary>
    /// A schedule belongs to its scene, and the scene belongs to a household — there is
    /// deliberately no direct Household link. Two routes to the same household would give SQL
    /// Server two cascade paths to this table, which it rejects.
    /// </summary>
    public LightScene Scene { get; set; } = null!;

    #endregion Properties

}
