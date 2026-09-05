namespace Home.WebUI.Infrastructure.Services.Security;

/// <summary>
/// Sends this circuit to the login page once its session turns out to be over.
/// <para>
/// This is not signing out, which nothing client-side is allowed to do: the sign-in cookie is the
/// session and only an HTTP response carrying <c>Set-Cookie</c> may clear it. It is navigation.
/// The login page renders statically, so going there is what gives the cookie a response that can
/// deal with it.
/// </para>
/// <para>
/// The case it exists for is a cookie that is still valid while the session behind it is gone: the
/// route authorises, the page renders, the navigation works, and every call for data fails. The
/// app looks alive and does nothing, which on a kitchen tablet is the worst way to fail.
/// </para>
/// </summary>
public interface ISessionEndedNavigator
{

    #region Methods

    /// <summary>
    /// Goes to the login page, carrying where the family were so they land back there. Safe to
    /// call from every one of the six requests a page fires at once: the first wins and the rest
    /// do nothing.
    /// </summary>
    void SessionHasEnded();

    #endregion Methods

}
