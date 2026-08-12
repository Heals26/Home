namespace Home.WebApi.UseCases.LightSchedules.CreateLightSchedule;

/// <summary>
/// DaysOfWeek is a bitmask — bit 0 is Sunday. 127 is every day, 62 is weekdays.
/// </summary>
public record CreateLightScheduleApiRequest(
    string Name,
    long LightSceneID,
    TimeSpan TimeOfDay,
    int DaysOfWeek);
