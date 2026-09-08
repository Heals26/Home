using AutoMapper;
using Home.Application.UseCases.CalendarSubscriptions.RefreshCalendarSubscription;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.CalendarSubscriptions.RefreshCalendarSubscription;

public class RefreshCalendarSubscriptionPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IRefreshCalendarSubscriptionOutputPort
{

    #region Methods

    Task IRefreshCalendarSubscriptionOutputPort.PresentCalendarSubscriptionNotFoundAsync(long calendarSubscriptionID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Calendar Subscription {calendarSubscriptionID} Not Found", cancellationToken);

    Task IRefreshCalendarSubscriptionOutputPort.PresentCalendarSubscriptionRefreshedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IRefreshCalendarSubscriptionOutputPort.PresentCalendarSubscriptionUnreachableAsync(CancellationToken cancellationToken)
        => this.ServiceUnavailableAsync("The calendar could not be read just now. Home is showing what it last fetched.", cancellationToken);

    #endregion Methods

}
