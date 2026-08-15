using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetActivityRegionBaseUrl(long activityRegionID)
        => $"{GetActivityRegionsBaseUrl()}/{activityRegionID}";

    private static string GetActivityRegionsBaseUrl()
        => $"{GetBaseApiUrl()}/ActivityRegions";

    #endregion Base

    #region Methods

    public static ApiProviderHelper CreateActivityRegion()
        => new(HttpMethod.Post, RouteType.Body, GetActivityRegionsBaseUrl());

    public static ApiProviderHelper DeleteActivityRegion(long activityRegionID)
        => new(HttpMethod.Delete, RouteType.Route, GetActivityRegionBaseUrl(activityRegionID));

    public static ApiProviderHelper GetActivityContents(long activityRegionID)
        => new(HttpMethod.Get, RouteType.Route, $"{GetActivityRegionBaseUrl(activityRegionID)}/Contents");

    public static ApiProviderHelper GetActivityRegion(long activityRegionID)
        => new(HttpMethod.Get, RouteType.Route, GetActivityRegionBaseUrl(activityRegionID));

    public static ApiProviderHelper UpdateActivityRegion(long activityRegionID)
        => new(HttpMethod.Patch, RouteType.Body, GetActivityRegionBaseUrl(activityRegionID));

    #endregion Methods

}
