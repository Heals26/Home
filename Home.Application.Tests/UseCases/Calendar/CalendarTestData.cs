using Home.Domain.Entities;
using Home.Domain.Enumerations;

namespace Home.Application.Tests.UseCases.Calendar;

/// <summary>
/// The builders the calendar tests share, so each test reads as what it is checking rather than
/// how an event is put together.
/// </summary>
internal static class CalendarTestData
{

    #region Methods

    public static CalendarEvent AllDayEvent(long calendarEventID, Household household, string title, DateOnly date, int lengthInDays = 0)
        => new()
        {
            CalendarEventID = calendarEventID,
            EndDate = date.AddDays(lengthInDays),
            Frequency = CalendarRecurrenceFrequency.None,
            Household = household,
            Interval = 1,
            IsAllDay = true,
            StartDate = date,
            Title = title
        };

    public static Activity Task(long activityID, Household household, string title, DateOnly dueDate, TimeSpan? dueTime = null, User? assignee = null)
        => new()
        {
            ActivityID = activityID,
            DueDateUTC = dueDate.ToDateTime(TimeOnly.MinValue),
            DueTime = dueTime,
            Household = household,
            Title = title,
            User = assignee
        };

    public static CalendarEvent TimedEvent(
        long calendarEventID,
        Household household,
        string title,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        string timeZoneID = "Australia/Brisbane")
        => new()
        {
            CalendarEventID = calendarEventID,
            EndDate = date,
            EndTime = endTime,
            Frequency = CalendarRecurrenceFrequency.None,
            Household = household,
            Interval = 1,
            StartDate = date,
            StartTime = startTime,
            TimeZoneID = timeZoneID,
            Title = title
        };

    public static MealPlanEntry Meal(long mealPlanEntryID, Household household, string title, DateOnly date, MealSlot? mealSlot = null)
        => new()
        {
            Date = date.ToDateTime(TimeOnly.MinValue),
            Household = household,
            MealPlanEntryID = mealPlanEntryID,
            MealSlot = mealSlot,
            Title = title
        };

    #endregion Methods

}
