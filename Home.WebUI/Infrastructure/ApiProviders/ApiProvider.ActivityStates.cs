using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetActivityStatesBaseUrl()
        => $"{GetBaseApiUrl()}/ActivityStates";

    #endregion Base

    #region Methods

    public static ApiProviderHelper GetActivityStates()
        => new(HttpMethod.Get, RouteType.Route, GetActivityStatesBaseUrl());

    #endregion Methods

}
