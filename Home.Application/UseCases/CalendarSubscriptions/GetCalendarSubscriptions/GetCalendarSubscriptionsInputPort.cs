using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.CalendarSubscriptions.GetCalendarSubscriptions;

public record GetCalendarSubscriptionsInputPort() : IInputPort<IGetCalendarSubscriptionsOutputPort>;
