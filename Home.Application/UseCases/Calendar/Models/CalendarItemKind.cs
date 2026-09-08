namespace Home.Application.UseCases.Calendar.Models;

/// <summary>
/// Where a thing on the calendar came from, which decides what a screen can do with it: an event
/// can be edited, a subscribed one cannot, a meal opens the planner, a task can be ticked off.
/// </summary>
public enum CalendarItemKind
{
    Event = 0,
    Subscribed = 1,
    Meal = 2,
    Task = 3,
}
