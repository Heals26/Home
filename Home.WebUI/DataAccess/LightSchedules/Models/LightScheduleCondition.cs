namespace Home.WebUI.DataAccess.LightSchedules.Models;

/// <summary>
/// A check made when a schedule comes due, deciding whether it fires or is skipped for the day.
/// Judged against the lights the scene itself touches, not the whole house.
/// </summary>
public enum LightScheduleCondition
{
    /// <summary>
    /// Fire whenever it is due.
    /// </summary>
    Always = 0,

    /// <summary>
    /// Fire only when none of the scene's lights are on.
    /// </summary>
    OnlyIfLightsAreOff = 1,

    /// <summary>
    /// Fire only when at least one of the scene's lights is on.
    /// </summary>
    OnlyIfLightsAreOn = 2,
}
