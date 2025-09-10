using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Methods

    public static ApiProviderHelper CreateOAuthToken()
        => new(HttpMethod.Post, RouteType.Form, "OAuth");

    #endregion Methods

}
