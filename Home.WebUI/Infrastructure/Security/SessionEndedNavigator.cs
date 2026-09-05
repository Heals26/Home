using Home.WebUI.Infrastructure.Services.Security;
using Home.WebUI.Infrastructure.UriProvider;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Infrastructure.Security;

/// <inheritdoc cref="ISessionEndedNavigator"/>
public class SessionEndedNavigator(NavigationManager navigationManager) : ISessionEndedNavigator
{

    #region Fields

    // Scoped to the circuit, so this is per browser tab. A page asks the API for six things at
    // once and each of them fails the same way; without this the family get six navigations and,
    // before it, six identical error messages stacked up telling them something they cannot act on.
    private bool m_HasNavigated;

    #endregion Fields

    #region Methods

    void ISessionEndedNavigator.SessionHasEnded()
    {
        if (this.m_HasNavigated)
            return;

        this.m_HasNavigated = true;

        var _Current = $"/{navigationManager.ToBaseRelativePath(navigationManager.Uri)}";

        // Landing back on the login page is not somewhere to return to, and asking to return to it
        // from itself is a loop.
        var _ReturnUrl = _Current.StartsWith(AuthorisationUriProvider.GetLoginUri(), StringComparison.OrdinalIgnoreCase)
            ? string.Empty
            : _Current;

        // forceLoad because the login page renders statically: an interactive navigation would
        // route to it inside the circuit, where there is no HttpContext and no cookie to set.
        navigationManager.NavigateTo(AuthorisationUriProvider.GetLoginUri(_ReturnUrl), forceLoad: true);
    }

    #endregion Methods

}
