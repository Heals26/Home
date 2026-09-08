namespace Home.WebUI.DataAccess.CalendarSubscriptions.CreateCalendarSubscription;

public class CreateCalendarSubscriptionWebAppResponse
{

    #region Properties

    public long CalendarSubscriptionID { get; set; }

    /// <summary>
    /// Whether the first read of the feed worked. False means the subscription exists but the
    /// calendar has nothing from it yet.
    /// </summary>
    public bool WasFetched { get; set; }

    #endregion Properties

}
