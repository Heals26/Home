using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetActivityContentBaseUrl(long activityContentID)
        => $"{GetActivityContentsBaseUrl()}/{activityContentID}";

    private static string GetActivityContentsBaseUrl()
        => $"{GetBaseApiUrl()}/ActivityContents";

    #endregion Base

    #region Methods

    public static ApiProviderHelper CreateActivityContent()
        => new(HttpMethod.Post, RouteType.Body, GetActivityContentsBaseUrl());

    public static ApiProviderHelper DeleteActivityContent(long activityContentID)
        => new(HttpMethod.Delete, RouteType.Route, GetActivityContentBaseUrl(activityContentID));

    public static ApiProviderHelper UpdateActivityContent(long activityContentID)
        => new(HttpMethod.Patch, RouteType.Body, GetActivityContentBaseUrl(activityContentID));

    #endregion Methods

}
