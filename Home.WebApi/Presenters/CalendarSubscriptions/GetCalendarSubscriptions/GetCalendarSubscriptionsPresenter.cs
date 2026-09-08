using AutoMapper;
using Home.Application.UseCases.CalendarSubscriptions.GetCalendarSubscriptions;
using Home.Domain.Entities;
using Home.WebApi.Infrastructure.Presenters;
using Home.WebApi.UseCases.CalendarSubscriptions.GetCalendarSubscriptions;
using Home.WebApi.UseCases.CalendarSubscriptions.Models;

namespace Home.WebApi.Presenters.CalendarSubscriptions.GetCalendarSubscriptions;

public class GetCalendarSubscriptionsPresenter(IMapper mapper)
    : OutputPortPresenter(mapper), IGetCalendarSubscriptionsOutputPort
{

    #region Methods

    Task IGetCalendarSubscriptionsOutputPort.PresentCalendarSubscriptionsAsync(IEnumerable<CalendarSubscription> subscriptions, CancellationToken cancellationToken)
        => this.OkAsync(new GetCalendarSubscriptionsApiResponse()
        {
            Subscriptions = [.. subscriptions.Select(s => new CalendarSubscriptionDto()
            {
                CalendarSubscriptionID = s.CalendarSubscriptionID,
                Host = HostOf(s.Url),
                LastError = s.LastError,
                LastFetchedUTC = s.LastFetchedUTC,
                Name = s.Name
            })]
        }, cancellationToken);

    private static string HostOf(string url)
        => Uri.TryCreate(url, UriKind.Absolute, out var _Uri) ? _Uri.Host : string.Empty;

    #endregion Methods

}
