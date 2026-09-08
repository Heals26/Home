namespace Home.WebUI.DataAccess.Calendar.Models;

/// <summary>
/// Where a thing on the calendar came from. Mirrors the API's enumeration value for value.
/// </summary>
public enum CalendarItemKind
{
    Event = 0,
    Subscribed = 1,
    Meal = 2,
    Task = 3,
}
