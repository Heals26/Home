using Home.WebApi.UseCases.CalendarSubscriptions.Models;

namespace Home.WebApi.UseCases.CalendarSubscriptions.GetCalendarSubscriptions;

public class GetCalendarSubscriptionsApiResponse
{

    #region Properties

    public List<CalendarSubscriptionDto> Subscriptions { get; set; } = [];

    #endregion Properties

}
