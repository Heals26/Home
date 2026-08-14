namespace Home.Domain.Enumerations;

/// <summary>
/// What starts a light schedule: a fixed local time, or the sun — computed from the
/// household's latitude and longitude on the day the schedule fires.
/// </summary>
public enum LightScheduleTrigger
{
    Time = 0,
    Sunrise = 1,
    Sunset = 2,
}
