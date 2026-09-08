using CleanArchitecture.Mediator;

namespace Home.Application.UseCases.CalendarSubscriptions.CreateCalendarSubscription;

/// <summary>
/// Subscribes the household to a read-only iCalendar feed. A blank <paramref name="Name"/> takes
/// the name the feed gives itself.
/// </summary>
public record CreateCalendarSubscriptionInputPort(string Name, string Url) : IInputPort<ICreateCalendarSubscriptionOutputPort>;
