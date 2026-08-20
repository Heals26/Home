using Home.Application.Infrastructure.Values;

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
    /// How long an access token stays valid. Owned by <see cref="SessionValues"/> now, because
    /// the refresh grant also reads it to decide when a token is worth replacing — this alias
    /// keeps the handler and both grant responses on the same number without touching them.
    /// </summary>
    public static readonly TimeSpan AccessTokenLifetime = SessionValues.AccessTokenLifetime;

    #endregion Properties

}
