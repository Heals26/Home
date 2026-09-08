using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.Calendar.GetCalendarEvent;

public record GetCalendarEventInputPort(long CalendarEventID) : IInputPort<IGetCalendarEventOutputPort>;
