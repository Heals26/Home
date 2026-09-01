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

    public static ApiProviderHelper DeleteUser(long userID)
        => new(HttpMethod.Delete, RouteType.Route, $"{GetUsersBaseUrl()}/{userID}");

    public static ApiProviderHelper GetUsers()
        => new(HttpMethod.Get, RouteType.Route, GetUsersBaseUrl());

    public static ApiProviderHelper UpdateUser(long userID)
        => new(HttpMethod.Patch, RouteType.Body, $"{GetUsersBaseUrl()}/{userID}");

    #endregion Methods

}
