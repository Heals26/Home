namespace Home.WebUI.Infrastructure.UriProvider;

public static class AuthorisationUriProvider
{

    #region Methods

    public static string GetLoginUri()
        => "/login";

    public static string GetLoginUri(string returnUrl)
        => $"{GetLoginUri()}?returnUrl={Uri.EscapeDataString(returnUrl)}";

    public static string GetLogoutUri()
        => "/logout";

    #endregion Methods

}
