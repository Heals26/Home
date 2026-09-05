namespace Home.WebUI.Infrastructure.Values;

public static class AuthorisationValues
{

    #region Fields

    /// <summary>
    /// The grant type the API's token endpoint matches on to route a request into the refresh
    /// branch — <c>OAuthValues.GrantTypeRefresh</c>. Sending the configured sign-in grant type
    /// here instead routes the refresh into the password branch, which 401s for want of a
    /// username and password.
    /// </summary>
    public const string RefreshGrantType = "refresh_token";

    /// <summary>
    /// The code the token endpoint answers with when it refused this installation rather than the
    /// person signing in. It means the client application row and the configured credentials do
    /// not match, and no amount of retyping a password will help.
    /// </summary>
    public const string InvalidClientError = "invalid_client";

    #endregion Fields

}
