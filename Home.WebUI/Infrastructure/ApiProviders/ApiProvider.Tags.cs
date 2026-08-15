using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetTagBaseUrl(long tagID)
        => $"{GetTagsBaseUrl()}/{tagID}";

    private static string GetTagsBaseUrl()
        => $"{GetBaseApiUrl()}/Tags";

    #endregion Base

    #region Methods

    public static ApiProviderHelper CreateTag()
        => new(HttpMethod.Post, RouteType.Body, GetTagsBaseUrl());

    public static ApiProviderHelper DeleteTag(long tagID)
        => new(HttpMethod.Delete, RouteType.Route, GetTagBaseUrl(tagID));

    public static ApiProviderHelper GetTags()
        => new(HttpMethod.Get, RouteType.Route, GetTagsBaseUrl());

    public static ApiProviderHelper UpdateTag(long tagID)
        => new(HttpMethod.Patch, RouteType.Body, GetTagBaseUrl(tagID));

    #endregion Methods

}
