namespace Home.WebUI.DataAccess.CalendarSubscriptions.CreateCalendarSubscription;

public class CreateCalendarSubscriptionWebAppRequest
{

    #region Properties

    /// <summary>
    /// What to call it in Home. Blank takes the name the feed gives itself.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The feed's secret address, https:// or webcal://.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    #endregion Properties

}
