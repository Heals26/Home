using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetLightGroupBaseUrl(long lightGroupID)
        => $"{GetLightGroupsBaseUrl()}/{lightGroupID}";

    private static string GetLightGroupsBaseUrl()
        => $"{GetBaseApiUrl()}/LightGroups";

    #endregion Base

    #region Methods

    public static ApiProviderHelper AssignLightToGroup(long lightGroupID)
        => new(HttpMethod.Put, RouteType.Body, $"{GetLightGroupBaseUrl(lightGroupID)}/lights");

    public static ApiProviderHelper CreateLightGroup()
        => new(HttpMethod.Post, RouteType.Body, GetLightGroupsBaseUrl());

    public static ApiProviderHelper DeleteLightGroup(long lightGroupID)
        => new(HttpMethod.Delete, RouteType.Route, GetLightGroupBaseUrl(lightGroupID));

    public static ApiProviderHelper SetLightGroupState(long lightGroupID)
        => new(HttpMethod.Patch, RouteType.Body, $"{GetLightGroupBaseUrl(lightGroupID)}/state");

    public static ApiProviderHelper UpdateLightGroup(long lightGroupID)
        => new(HttpMethod.Patch, RouteType.Body, GetLightGroupBaseUrl(lightGroupID));

    #endregion Methods

}
