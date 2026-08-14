namespace Home.WebUI.DataAccess.LightSchedules.Models;

public class LightScheduleDto
{

    #region Properties

    /// <summary>
    /// Bitmask of days — bit 0 is Sunday, matching <see cref="DayOfWeek"/>.
    /// </summary>
    public int DaysOfWeek { get; set; }

    /// <summary>
    /// Whether the schedule fires at all.
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// When it last fired, or null if it never has.
    /// </summary>
    public DateTime? LastRunUTC { get; set; }

    /// <summary>
    /// The ID of the scene this schedule applies.
    /// </summary>
    public long LightSceneID { get; set; }

    /// <summary>
    /// The ID of the schedule.
    /// </summary>
    public long LightScheduleID { get; set; }

    /// <summary>
    /// The schedule's name, e.g. "Wind down".
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Minutes relative to the sun event for sunrise and sunset triggers; negative is before.
    /// </summary>
    public int OffsetMinutes { get; set; }

    /// <summary>
    /// The name of the scene this schedule applies.
    /// </summary>
    public string SceneName { get; set; } = string.Empty;

    /// <summary>
    /// Local time of day the schedule fires at, when the trigger is a fixed time.
    /// </summary>
    public TimeSpan TimeOfDay { get; set; }

    /// <summary>
    /// What starts the schedule: a fixed time, sunrise, or sunset.
    /// </summary>
    public LightScheduleTrigger Trigger { get; set; }

    #endregion Properties

}
