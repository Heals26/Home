namespace Home.Domain.Enumerations;

/// <summary>
/// A check made when a schedule comes due, deciding whether it fires or is skipped for the day.
/// Judged against the lights the scene itself touches, not the whole house.
/// </summary>
public enum LightScheduleCondition
{
    /// <summary>
    /// Fire whenever it is due. What every schedule did before conditions existed.
    /// </summary>
    Always = 0,

    /// <summary>
    /// Fire only when none of the scene's lights are on. This is the one that stops an evening
    /// scene walking over a room somebody has already lit the way they wanted it.
    /// </summary>
    OnlyIfLightsAreOff = 1,

    /// <summary>
    /// Fire only when at least one of the scene's lights is on. For a schedule that changes a room
    /// rather than lights it: dimming for bedtime is pointless in a room nobody is in.
    /// </summary>
    OnlyIfLightsAreOn = 2,
}
