using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetHouseholdsBaseUrl()
        => $"{GetBaseApiUrl()}/Households";

    #endregion Base

    #region Methods

    public static ApiProviderHelper GetHouseholdSettings()
        => new(HttpMethod.Get, RouteType.Route, $"{GetHouseholdsBaseUrl()}/settings");

    public static ApiProviderHelper GetSetupStatus()
        => new(HttpMethod.Get, RouteType.Route, $"{GetHouseholdsBaseUrl()}/setup-status", AllowsAnonymous: true);

    public static ApiProviderHelper RegisterHousehold()
        => new(HttpMethod.Post, RouteType.Body, $"{GetHouseholdsBaseUrl()}/register", AllowsAnonymous: true);

    public static ApiProviderHelper UpdateHouseholdSettings()
        => new(HttpMethod.Patch, RouteType.Body, $"{GetHouseholdsBaseUrl()}/settings");

    #endregion Methods

}
