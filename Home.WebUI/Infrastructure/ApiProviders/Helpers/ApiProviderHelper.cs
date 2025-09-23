namespace Home.WebUI.Infrastructure.ApiProviders.Helpers;

public record ApiProviderHelper(HttpMethod HttpMethod, RouteType RouteType, string Uri, string? Version = "1.0");
