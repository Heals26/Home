using Home.WebUI.Infrastructure.UriProvider;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;

namespace Home.WebUI.Components.Pages.Authorisation;

public partial class Logout
{

    #region Properties

    /// <summary>
    /// Supplied on the static render. Null means the interactive router got here first, and
    /// initialisation re-enters over plain HTTP.
    /// </summary>
    [CascadingParameter] public HttpContext? HttpContext { get; set; }

    #endregion Properties

    #region Lifecycle Methods

    protected override void OnInitialized()
    {
        if (this.HttpContext == null)
            this.NavigationManager.NavigateTo(this.NavigationManager.Uri, forceLoad: true);
    }

    #endregion Lifecycle Methods

    #region Methods

    private async Task SignOutAsync()
    {
        if (this.HttpContext == null)
            return;

        await this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        this.NavigationManager.NavigateTo(AuthorisationUriProvider.GetLoginUri());
    }

    #endregion Methods

}
