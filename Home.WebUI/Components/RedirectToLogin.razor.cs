using Home.WebUI.Infrastructure.UriProvider;

namespace Home.WebUI.Components;

public partial class RedirectToLogin
{

    #region Lifecycle Methods

    protected override void OnInitialized()
        => this.NavigationManager.NavigateTo(AuthorisationUriProvider.GetLoginUri(Uri.EscapeDataString(this.NavigationManager.Uri)), forceLoad: true);

    #endregion Lifecycle Methods

}
