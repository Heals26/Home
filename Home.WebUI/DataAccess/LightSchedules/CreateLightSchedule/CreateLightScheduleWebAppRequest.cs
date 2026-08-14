using Home.WebUI.DataAccess.LightSchedules.Models;

namespace Home.WebUI.DataAccess.LightSchedules.CreateLightSchedule;

public class CreateLightScheduleWebAppRequest
{

    #region Properties

    /// <summary>
    /// Bitmask of days — bit 0 is Sunday. 127 is every day, 62 is weekdays.
    /// </summary>
    public int DaysOfWeek { get; set; }

    /// <summary>
    /// The scene the schedule applies when it fires.
    /// </summary>
    public long LightSceneID { get; set; }

    /// <summary>
    /// The schedule's name, e.g. "Wind down".
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Minutes relative to the sun event for sunrise and sunset triggers; negative is before.
    /// </summary>
    public int OffsetMinutes { get; set; }

    /// <summary>
    /// Local time of day the schedule fires at, when the trigger is a fixed time.
    /// </summary>
    public TimeSpan TimeOfDay { get; set; }

    /// <summary>
    /// What starts the schedule. Sun triggers need the household's location set in Settings.
    /// </summary>
    public LightScheduleTrigger Trigger { get; set; }

    #endregion Properties

}
