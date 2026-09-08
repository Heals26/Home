using Home.WebUI.Infrastructure.ApiProviders.Helpers;
using System.Globalization;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetCalendarBaseUrl()
        => $"{GetBaseApiUrl()}/Calendar";

    private static string GetCalendarEventBaseUrl(long calendarEventID)
        => $"{GetCalendarBaseUrl()}/Events/{calendarEventID}";

    private static string GetCalendarEventsBaseUrl()
        => $"{GetCalendarBaseUrl()}/Events";

    private static string GetCalendarSubscriptionBaseUrl(long calendarSubscriptionID)
        => $"{GetCalendarSubscriptionsBaseUrl()}/{calendarSubscriptionID}";

    private static string GetCalendarSubscriptionsBaseUrl()
        => $"{GetBaseApiUrl()}/CalendarSubscriptions";

    #endregion Base

    #region Methods

    public static ApiProviderHelper CreateCalendarEvent()
        => new(HttpMethod.Post, RouteType.Body, GetCalendarEventsBaseUrl());

    public static ApiProviderHelper CreateCalendarSubscription()
        => new(HttpMethod.Post, RouteType.Body, GetCalendarSubscriptionsBaseUrl());

    /// <summary>
    /// Without <paramref name="occurrenceDate"/> the whole event goes; with it, on a repeating
    /// event, only that occurrence is skipped.
    /// </summary>
    public static ApiProviderHelper DeleteCalendarEvent(long calendarEventID, DateOnly? occurrenceDate = null)
        => new(HttpMethod.Delete, RouteType.Route, occurrenceDate == null
            ? GetCalendarEventBaseUrl(calendarEventID)
            : $"{GetCalendarEventBaseUrl(calendarEventID)}?occurrenceDate={IsoDate(occurrenceDate.Value)}");

    public static ApiProviderHelper DeleteCalendarSubscription(long calendarSubscriptionID)
        => new(HttpMethod.Delete, RouteType.Route, GetCalendarSubscriptionBaseUrl(calendarSubscriptionID));

    public static ApiProviderHelper GetCalendar(DateOnly fromDate, DateOnly toDate, string timeZoneID)
        => new(HttpMethod.Get, RouteType.Route, $"{GetCalendarBaseUrl()}?fromDate={IsoDate(fromDate)}&toDate={IsoDate(toDate)}&timeZoneID={Uri.EscapeDataString(timeZoneID)}");

    public static ApiProviderHelper GetCalendarEvent(long calendarEventID)
        => new(HttpMethod.Get, RouteType.Route, GetCalendarEventBaseUrl(calendarEventID));

    public static ApiProviderHelper GetCalendarSubscriptions()
        => new(HttpMethod.Get, RouteType.Route, GetCalendarSubscriptionsBaseUrl());

    public static ApiProviderHelper RefreshCalendarSubscription(long calendarSubscriptionID)
        => new(HttpMethod.Post, RouteType.Route, $"{GetCalendarSubscriptionBaseUrl(calendarSubscriptionID)}/refresh");

    public static ApiProviderHelper UpdateCalendarEvent(long calendarEventID)
        => new(HttpMethod.Put, RouteType.Body, GetCalendarEventBaseUrl(calendarEventID));

    private static string IsoDate(DateOnly date)
        => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    #endregion Methods

}
