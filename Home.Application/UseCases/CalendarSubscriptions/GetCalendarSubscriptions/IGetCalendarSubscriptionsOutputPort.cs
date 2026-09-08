using Home.Domain.Entities;

namespace Home.Application.UseCases.CalendarSubscriptions.GetCalendarSubscriptions;

public interface IGetCalendarSubscriptionsOutputPort
{

    #region Methods

    Task PresentCalendarSubscriptionsAsync(IEnumerable<CalendarSubscription> subscriptions, CancellationToken cancellationToken);

    #endregion Methods

}
