using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetWeatherBaseUrl()
        => $"{GetBaseApiUrl()}/Weather";

    #endregion Base

    #region Methods

    public static ApiProviderHelper GetWeather()
        => new(HttpMethod.Get, RouteType.Route, GetWeatherBaseUrl());

    #endregion Methods

}
