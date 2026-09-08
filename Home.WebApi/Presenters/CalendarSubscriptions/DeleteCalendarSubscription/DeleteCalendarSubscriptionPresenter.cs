using AutoMapper;
using Home.Application.UseCases.CalendarSubscriptions.DeleteCalendarSubscription;
using Home.WebApi.Infrastructure.Presenters;

namespace Home.WebApi.Presenters.CalendarSubscriptions.DeleteCalendarSubscription;

public class DeleteCalendarSubscriptionPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IDeleteCalendarSubscriptionOutputPort
{

    #region Methods

    Task IDeleteCalendarSubscriptionOutputPort.PresentCalendarSubscriptionDeletedAsync(CancellationToken cancellationToken)
        => this.NoContentAsync(cancellationToken);

    Task IDeleteCalendarSubscriptionOutputPort.PresentCalendarSubscriptionNotFoundAsync(long calendarSubscriptionID, CancellationToken cancellationToken)
        => this.NotFoundAsync($"Calendar Subscription {calendarSubscriptionID} Not Found", cancellationToken);

    #endregion Methods

}
