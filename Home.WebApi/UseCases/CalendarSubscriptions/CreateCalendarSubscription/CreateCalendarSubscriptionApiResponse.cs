namespace Home.WebApi.UseCases.CalendarSubscriptions.CreateCalendarSubscription;

public class CreateCalendarSubscriptionApiResponse
{

    #region Properties

    public long CalendarSubscriptionID { get; set; }

    /// <summary>
    /// Whether the first read of the feed worked. False means the subscription exists but the
    /// calendar has nothing from it yet; its card says why.
    /// </summary>
    public bool WasFetched { get; set; }

    #endregion Properties

}
