namespace Home.WebApi.UseCases.CalendarSubscriptions.CreateCalendarSubscription;

/// <summary>
/// A blank <c>Name</c> takes the name the feed gives itself.
/// </summary>
public record CreateCalendarSubscriptionApiRequest(string Name, string Url);
