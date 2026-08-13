using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetLightScheduleBaseUrl(long lightScheduleID)
        => $"{GetLightSchedulesBaseUrl()}/{lightScheduleID}";

    private static string GetLightSchedulesBaseUrl()
        => $"{GetBaseApiUrl()}/LightSchedules";

    #endregion Base

    #region Methods

    public static ApiProviderHelper CreateLightSchedule()
        => new(HttpMethod.Post, RouteType.Body, GetLightSchedulesBaseUrl());

    public static ApiProviderHelper DeleteLightSchedule(long lightScheduleID)
        => new(HttpMethod.Delete, RouteType.Route, GetLightScheduleBaseUrl(lightScheduleID));

    public static ApiProviderHelper GetLightSchedules()
        => new(HttpMethod.Get, RouteType.Route, GetLightSchedulesBaseUrl());

    public static ApiProviderHelper UpdateLightSchedule(long lightScheduleID)
        => new(HttpMethod.Patch, RouteType.Body, GetLightScheduleBaseUrl(lightScheduleID));

    #endregion Methods

}
