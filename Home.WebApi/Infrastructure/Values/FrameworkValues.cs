namespace Home.WebApi.Infrastructure.Values;

public static class FrameworkValues
{

    #region Properties

    public static string Authorisation = "Authorization";
    public static string Basic = "Basic";
    public static string Bearer = "Bearer";
    public static string Flexible = "Flexible";

    public static string IdentityClaimScopes = "Scopes";

    public const string ScopeWebApp = "WebApp";

    /// <summary>
    /// How long an access token stays valid. Was an inline AddHours(1) in three places — the
    /// handler that rejects expired tokens and both grant responses that report the remaining
    /// seconds — which is exactly the sort of thing that drifts apart.
    /// </summary>
    public static readonly TimeSpan AccessTokenLifetime = TimeSpan.FromHours(1);

    #endregion Properties

}
