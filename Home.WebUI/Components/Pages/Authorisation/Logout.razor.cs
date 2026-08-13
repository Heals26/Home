using Home.WebUI.Infrastructure.UriProvider;

namespace Home.WebUI.Components.Pages.Authorisation;

public partial class Logout
{

    #region Lifecycle Methods

    protected override async Task OnInitializedAsync()
    {
        await this.AuthorisationService.SignOutAsync();
        this.NavigationManager.NavigateTo(AuthorisationUriProvider.GetLoginUri(), true);
    }

    #endregion Lifecycle Methods

}
