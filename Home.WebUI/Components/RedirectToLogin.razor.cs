using Home.WebUI.Infrastructure.UriProvider;

namespace Home.WebUI.Components;

public partial class RedirectToLogin
{

    #region Lifecycle Methods

    // The return URL is app-relative on purpose: the login page refuses anything absolute, so
    // handing it the full address would lose the "land back where you started" behaviour.
    protected override void OnInitialized()
        => this.NavigationManager.NavigateTo(
            AuthorisationUriProvider.GetLoginUri($"/{this.NavigationManager.ToBaseRelativePath(this.NavigationManager.Uri)}"),
            forceLoad: true);

    #endregion Lifecycle Methods

}
