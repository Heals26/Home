namespace Home.WebUI.DataAccess.LightSchedules.Models;

/// <summary>
/// What starts a light schedule: a fixed local time, or the sun — mirrored from the API's
/// numeric values.
/// </summary>
public enum LightScheduleTrigger
{
    Time = 0,
    Sunrise = 1,
    Sunset = 2,
}
