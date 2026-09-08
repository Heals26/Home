using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Calendar.DeleteCalendarEvent;

/// <summary>
/// Removes an event, or with <paramref name="OccurrenceDate"/> set on a repeating one, skips just
/// that occurrence and leaves the series standing.
/// </summary>
public record DeleteCalendarEventInputPort(long CalendarEventID, DateOnly? OccurrenceDate) : IInputPort<IDeleteCalendarEventOutputPort>;
