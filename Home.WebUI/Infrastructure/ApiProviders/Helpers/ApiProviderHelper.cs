namespace Home.WebUI.Infrastructure.ApiProviders.Helpers;

/// <summary>
/// AllowsAnonymous marks the handful of endpoints callable before sign-in (setup status,
/// first-run registration) — the client sends them with no Authorization header.
/// </summary>
public record ApiProviderHelper(HttpMethod HttpMethod, RouteType RouteType, string Uri, string? Version = "1.0", bool AllowsAnonymous = false);
