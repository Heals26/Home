namespace Home.Domain.Enumerations;

/// <summary>
/// How often a calendar event repeats. Deliberately small: the exceptions to a series are what
/// make recurrence hard, and those live in <c>CalendarEventException</c>, not here.
/// </summary>
public enum CalendarRecurrenceFrequency
{
    None = 0,
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Yearly = 4,
}
