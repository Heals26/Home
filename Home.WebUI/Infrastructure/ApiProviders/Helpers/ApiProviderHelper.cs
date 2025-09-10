namespace Home.WebUI.Infrastructure.ApiProviders.Helpers;

public class ApiProviderHelper(HttpMethod httpMethod, RouteType routeType, string uri, string? version = null)
{

    #region Properties

    public HttpMethod HttpMethod { get; } = httpMethod;
    public RouteType RouteType { get; } = routeType;
    public Uri Uri { get; } = new(uri, UriKind.RelativeOrAbsolute);
    public string Version { get; } = version ?? "1.0";

    #endregion Properties

}
