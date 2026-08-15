using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetActivityBaseUrl(long activityID)
        => $"{GetActivitiesBaseUrl()}/{activityID}";

    private static string GetActivitiesBaseUrl()
        => $"{GetBaseApiUrl()}/Activities";

    #endregion Base

    #region Methods

    public static ApiProviderHelper CreateActivity()
        => new(HttpMethod.Post, RouteType.Body, GetActivitiesBaseUrl());

    public static ApiProviderHelper DeleteActivity(long activityID)
        => new(HttpMethod.Delete, RouteType.Route, GetActivityBaseUrl(activityID));

    public static ApiProviderHelper GetActivities()
        => new(HttpMethod.Get, RouteType.Route, GetActivitiesBaseUrl());

    public static ApiProviderHelper GetActivity(long activityID)
        => new(HttpMethod.Get, RouteType.Route, GetActivityBaseUrl(activityID));

    public static ApiProviderHelper GetActivityRegions(long activityID)
        => new(HttpMethod.Get, RouteType.Route, $"{GetActivityBaseUrl(activityID)}/Regions");

    /// <summary>
    /// Replaces the card's labels with the set sent — anything left out is taken off the card.
    /// </summary>
    public static ApiProviderHelper SetActivityTags(long activityID)
        => new(HttpMethod.Put, RouteType.Body, $"{GetActivityBaseUrl(activityID)}/Tags");

    public static ApiProviderHelper UpdateActivity(long activityID)
        => new(HttpMethod.Patch, RouteType.Body, GetActivityBaseUrl(activityID));

    #endregion Methods

}
