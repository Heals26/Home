namespace Home.WebUI.Infrastructure.ApiProviders.Helpers;

/// <summary>
/// AllowsAnonymous marks the handful of endpoints callable before sign-in (setup status,
/// first-run registration), which the client sends with no Authorization header. UndoToken is only
/// ever set by the undo logic, for a request the bar may offer to take back.
/// </summary>
public record ApiProviderHelper(HttpMethod HttpMethod, RouteType RouteType, string Uri, string? Version = "1.0", bool AllowsAnonymous = false, Guid? UndoToken = null);
