using Home.WebUI.Infrastructure.ApiProviders.Helpers;

namespace Home.WebUI.Infrastructure.ApiProviders;

public static partial class ApiProvider
{

    #region Base

    private static string GetUsersBaseUrl()
        => $"{GetBaseApiUrl()}/Users";

    #endregion Base

    #region Methods

    public static ApiProviderHelper CreateUser()
        => new(HttpMethod.Post, RouteType.Body, GetUsersBaseUrl());

    public static ApiProviderHelper GetUsers()
        => new(HttpMethod.Get, RouteType.Route, GetUsersBaseUrl());

    #endregion Methods

}
