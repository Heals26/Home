using AutoMapper;
using Home.Application.UseCases.CalendarSubscriptions.CreateCalendarSubscription;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.CalendarSubscriptions.CreateCalendarSubscription;

namespace Home.WebApi.Presenters.CalendarSubscriptions.CreateCalendarSubscription;

public class CreateCalendarSubscriptionPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), ICreateCalendarSubscriptionOutputPort
{

    #region Methods

    Task ICreateCalendarSubscriptionOutputPort.PresentCalendarSubscriptionCreatedAsync(long calendarSubscriptionID, bool wasFetched, CancellationToken cancellationToken)
        => this.CreatedAsync(
            calendarSubscriptionID,
            new CreateCalendarSubscriptionApiResponse() { CalendarSubscriptionID = calendarSubscriptionID, WasFetched = wasFetched },
            cancellationToken);

    #endregion Methods

}
