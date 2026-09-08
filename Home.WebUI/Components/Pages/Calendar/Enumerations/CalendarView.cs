namespace Home.WebUI.Components.Pages.Calendar.Enumerations;

/// <summary>
/// How the calendar page lays the days out. Month and Week are grids; List is the same week as a
/// single column, which is what a phone can carry.
/// </summary>
public enum CalendarView
{
    Month = 0,
    Week = 1,
    List = 2,
}
