using Home.WebUI.DataAccess.CalendarSubscriptions.Models;

namespace Home.WebUI.DataAccess.CalendarSubscriptions.GetCalendarSubscriptions;

public class GetCalendarSubscriptionsWebAppResponse
{

    #region Properties

    public List<CalendarSubscriptionDto> Subscriptions { get; set; } = [];

    #endregion Properties

}
