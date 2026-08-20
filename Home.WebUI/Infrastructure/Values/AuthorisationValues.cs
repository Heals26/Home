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

    #endregion Fields

}
