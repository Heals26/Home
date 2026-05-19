using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    public static string GetBaseApiUrl()
        => "api";

    #endregion Base

    #region Methods

    public static ApiProviderHelper GetOAuthToken()
        => new(HttpMethod.Post, RouteType.Form, $"{GetBaseApiUrl()}/OAuth");

    public static ApiProviderHelper GetRefreshToken()
        => new(HttpMethod.Post, RouteType.Form, $"{GetBaseApiUrl()}/OAuth");

    #endregion Methods

}
