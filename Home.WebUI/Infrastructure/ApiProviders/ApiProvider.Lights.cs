using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetLightBaseUrl(string lightID)
        => $"{GetLightsBaseUrl()}/{lightID}";

    private static string GetLightsBaseUrl()
        => $"{GetBaseApiUrl()}/Lights";

    #endregion Base

    #region Methods

    public static ApiProviderHelper GetLights()
        => new(HttpMethod.Get, RouteType.Route, GetLightsBaseUrl());

    public static ApiProviderHelper SetLightState(string lightID)
        => new(HttpMethod.Patch, RouteType.Body, GetLightBaseUrl(lightID));

    public static ApiProviderHelper StartLightEffect()
        => new(HttpMethod.Post, RouteType.Body, $"{GetLightsBaseUrl()}/effects");

    public static ApiProviderHelper SyncLights()
        => new(HttpMethod.Post, RouteType.Route, $"{GetLightsBaseUrl()}/sync");

    #endregion Methods

}
