namespace Home.WebUI.DataAccess.Calendar.Models;

/// <summary>
/// How often an event repeats. Mirrors the API's enumeration value for value.
/// </summary>
public enum CalendarRecurrenceFrequency
{
    None = 0,
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Yearly = 4,
}
