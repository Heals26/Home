using Home.Domain.Enumerations;

namespace Home.WebApi.UseCases.LightSchedules.CreateLightSchedule;

/// <summary>
/// DaysOfWeek is a bitmask — bit 0 is Sunday. 127 is every day, 62 is weekdays.
/// Trigger 0 fires at TimeOfDay; 1 (sunrise) and 2 (sunset) fire at the sun event plus
/// OffsetMinutes, and need the household's location to be set.
/// </summary>
public record CreateLightScheduleApiRequest(
    string Name,
    long LightSceneID,
    LightScheduleTrigger Trigger,
    TimeSpan TimeOfDay,
    int OffsetMinutes,
    int DaysOfWeek);
