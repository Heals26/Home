namespace Home.WebApi.UseCases.LightSchedules.Models;

public class LightScheduleDto
{

    #region Properties

    /// <summary>
    /// Bitmask of days — bit 0 is Sunday, matching <see cref="System.DayOfWeek"/>.
    /// </summary>
    public int DaysOfWeek { get; set; }

    public bool IsEnabled { get; set; }

    /// <summary>
    /// When it last fired, or null if it never has.
    /// </summary>
    public DateTime? LastRunUTC { get; set; }

    public long LightScheduleID { get; set; }

    public long LightSceneID { get; set; }

    public string Name { get; set; }

    /// <summary>
    /// The name of the scene this schedule applies.
    /// </summary>
    public string SceneName { get; set; }

    /// <summary>
    /// Local time of day the schedule fires at.
    /// </summary>
    public TimeSpan TimeOfDay { get; set; }

    #endregion Properties

}
