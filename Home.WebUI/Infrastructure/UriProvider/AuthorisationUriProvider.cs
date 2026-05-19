namespace Home.WebUI.Infrastructure.UriProvider;

public static class AuthorisationUriProvider
{

    #region Methods

    public static string GetLoginUri()
        => "/";

    public static string GetLoginUri(string returnUrl)
        => string.IsNullOrWhiteSpace(returnUrl)
            ? GetLoginUri()
            : $"{GetLoginUri()}?returnUrl={Uri.EscapeDataString(returnUrl)}";

    public static string GetLogoutUri()
        => "/logout";

    #endregion Methods

}
