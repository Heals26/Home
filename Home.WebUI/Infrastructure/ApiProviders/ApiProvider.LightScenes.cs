using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetLightSceneBaseUrl(long lightSceneID)
        => $"{GetLightScenesBaseUrl()}/{lightSceneID}";

    private static string GetLightScenesBaseUrl()
        => $"{GetBaseApiUrl()}/LightScenes";

    #endregion Base

    #region Methods

    public static ApiProviderHelper ApplyLightScene(long lightSceneID)
        => new(HttpMethod.Post, RouteType.Route, $"{GetLightSceneBaseUrl(lightSceneID)}/apply");

    public static ApiProviderHelper CaptureLightScene()
        => new(HttpMethod.Post, RouteType.Body, GetLightScenesBaseUrl());

    public static ApiProviderHelper DeleteLightScene(long lightSceneID)
        => new(HttpMethod.Delete, RouteType.Route, GetLightSceneBaseUrl(lightSceneID));

    public static ApiProviderHelper GetLightScenes()
        => new(HttpMethod.Get, RouteType.Route, GetLightScenesBaseUrl());

    public static ApiProviderHelper SetLightSceneSequence(long lightSceneID)
        => new(HttpMethod.Patch, RouteType.Body, $"{GetLightSceneBaseUrl(lightSceneID)}/sequence");

    #endregion Methods

}
